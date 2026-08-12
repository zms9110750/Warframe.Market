using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http.Resilience;
using Polly.RateLimiting;
using Polly.Telemetry;
using Serilog.Events;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Services;
using zms9110750.Warframe.Market.GUI.Services;

namespace zms9110750.Warframe.Market.GUI;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // Serilog 最先初始化：任何阶段的日志（含启动早期异常）都能落盘
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)     // HttpClient 每次请求日志（噪音）
            .MinimumLevel.Override("ZiggyCreatures.Caching.Fusion", LogEventLevel.Warning) // FusionCache 每次 GetOrDefault/SetAsync（噪音）
            .MinimumLevel.Override("Polly", LogEventLevel.Warning)                // 保留重试(OnRetry)/限流拒绝(Error)，滤掉 ExecutionAttempted
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "logs", "app.log"))
            .CreateLogger();
        Log.Information("=== 程序启动 ===");

        // 全局异常兜底：任何未处理的 .NET 异常都写日志，避免静默崩溃
        AppDomain.CurrentDomain.UnhandledException += (_, e) => {
            try { Log.Fatal(e.ExceptionObject as Exception, "未处理的 AppDomain 异常"); }
            catch { }
        };
        TaskScheduler.UnobservedTaskException += (_, e) => {
            // PhotinoX/BlazorWebView 启动期渲染器竞态（"no browser renderer with ID"）是已知噪音，
            // UI 稳定后不再出现，静音避免日志污染；其余未观察异常照常记录
            if (e.Exception.ToString().Contains("no browser renderer with ID", StringComparison.OrdinalIgnoreCase))
            {
                Log.Debug("忽略 PhotinoX 渲染器竞态噪音");
                e.SetObserved();
                return;
            }
            Log.Error(e.Exception, "未观察的任务异常");
            e.SetObserved();
        };

        var appBuilder = PhotinoBlazorAppBuilder.CreateDefault(args);
        Log.Information("PhotinoBlazorAppBuilder 已创建");

        // 微软配置系统：appsettings.json（Logging + Gui + Version 节）→ IConfiguration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        // GUI 全局配置（集中可改）：从 IConfiguration 的 Gui 节绑定，DI 注册单例
        GuiConfig config;
        try
        {
            config = configuration.GetSection("Gui").Get<GuiConfig>() ?? new GuiConfig();
        }
        catch (Exception ex)
        {
            // TimeSpan 等绑定失败（用户手改 appsettings.json 非法值）→ 回退默认，不启动崩溃
            Log.Warning(ex, "Gui 配置绑定失败，使用默认值");
            config = new GuiConfig();
        }
        appBuilder.Services.AddSingleton(config);
        appBuilder.Services.AddSingleton<IConfiguration>(configuration);
        CacheConfig.Ttl = config.CacheDefaultTtl;
        CacheConfig.UserTtl = config.CacheUserTtl;
        Log.Information("AppConfig 注册: API={Api}, 限流 {Permit}/s Queue={Queue}, 429重试 {429Max}, 版本={Version}",
            config.ApiBaseAddress, config.RateLimitPermit, config.RateLimitQueue, config.Http429RetryMax,
            configuration["Version:Program"]);

        appBuilder.Services.AddMasaBlazor();
        Log.Information("AddMasaBlazor 完成");

        // FusionCache（Sqlite 分布式持久化）→ HybridCache 桥接，供下方 HTTP 缓存使用
        appBuilder.Services.AddSqliteCache(config.CacheDbFile)
            .AddFusionCache()
            .WithRegisteredDistributedCache()
            .WithSerializer(new FusionCacheSystemTextJsonSerializer(new JsonSerializerOptions {
                Converters = { new HttpResponseMessageJsonConverter() },
            }))
            .AsHybridCache();

        // Warframe.Market HTTP 客户端：缓存(命中跳过后续策略) → 限流感知重试(本地限流异常/429) → 令牌桶限流 3/s
        // HTTP 响应缓存键 = {method}/{scheme}/{host}{path}，GET 全自动缓存，无需业务层管缓存
        appBuilder.Services.AddHttpClient("wfm", c => c.BaseAddress = new Uri(config.ApiBaseAddress))
            .AddResilienceHandler("wfm", (pipeline, ctx) => {
                // Polly 遥测：策略事件经 ILoggerFactory（AddSerilog 提供）写进 Serilog。
                // 事件：OnRetry（429/限流重试，含 AttemptNumber/Delay/Result 状态码）、
                // OnRateLimiterRejected（本地队列满）、CacheMissed/CacheHit（缓存行为）——诊断 429 与限流用。
                pipeline.ConfigureTelemetry(ctx.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());

                pipeline.AddCaching(new HttpCachingStrategyOptions {
                    HybridCache = ctx.ServiceProvider.GetRequiredService<HybridCache>(),
                    CacheKeyProvider = CacheConfig.CacheKeyProvider,
                    // 非 200（404/429/5xx）不缓存——避免"未找到"/失败响应被缓存后长期命中
                    HybridCacheSetEntryOptionsProvider = (pipelineCtx, response) => new ValueTask<HybridCacheEntryOptions?>(
                        response.StatusCode != HttpStatusCode.OK
                            ? new HybridCacheEntryOptions {
                                Flags = HybridCacheEntryFlags.DisableLocalCacheWrite
                                      | HybridCacheEntryFlags.DisableDistributedCacheWrite,
                            }
                            : CacheConfig.EntryOptionsProvider(pipelineCtx.GetRequestMessage()?.RequestUri?.LocalPath ?? "")),
                });

                // 共享限流器（并发限流 3/s）：外层限流重试读取其队列数估算等待时间
                var limiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions {
                    PermitLimit = config.RateLimitPermit,
                    SegmentsPerWindow = 1,
                    Window = TimeSpan.FromSeconds(1),
                    QueueLimit = config.RateLimitQueue,
                });

                // ② 限流重试（外层）：本地并发限流拒绝（队列满）→ 重试，等队列腾出。
                //    只处理 RateLimiterRejectedException；429 交给最内层消化。
                pipeline.AddRetry(new RetryStrategyOptions<HttpResponseMessage> {
                    ShouldHandle = args => ValueTask.FromResult(args.Outcome.Exception is RateLimiterRejectedException),
                    MaxRetryAttempts = config.RateLimitRetryMax,
                    DelayGenerator = _ => new ValueTask<TimeSpan?>(TimeSpan.FromMilliseconds(((limiter.GetStatistics()?.CurrentQueuedCount ?? 0) * config.RateLimitRetryPerQueuedMs) + config.RateLimitRetryBaseMs)),
                    OnRetry = args => {
                        var url = args.Outcome.Result?.RequestMessage?.RequestUri?.AbsoluteUri ?? "?";
                        Log.Warning("限流重试（本地队列满）第 {Attempt} 次，等待 {Delay}ms: {Url}",
                            args.AttemptNumber, args.RetryDelay.TotalMilliseconds, url);
                        return default;
                    },
                });

                // ③ 并发限流（3/s）：放行后的请求才可能被服务器限流
                pipeline.AddRateLimiter(limiter);

                // ④ 429 重试（最内）：服务器限流 → 内部消化。
                //    Delay 等服务器限流窗口（3/s）重置；重试的请求在最内层，不重新经过并发限流。
                pipeline.AddRetry(new RetryStrategyOptions<HttpResponseMessage> {
                    ShouldHandle = args => ValueTask.FromResult(args.Outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests),
                    MaxRetryAttempts = config.Http429RetryMax,
                    DelayGenerator = _ => new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(config.Http429RetryDelayMs / 1000.0)),
                    OnRetry = args => {
                        var url = args.Outcome.Result?.RequestMessage?.RequestUri?.AbsoluteUri ?? "?";
                        Log.Warning("429 重试第 {Attempt} 次，等待 {Delay}ms: {Url}",
                            args.AttemptNumber, args.RetryDelay.TotalMilliseconds, url);
                        return default;
                    },
                });

                // ⑤ 空数据重试（最内）：API 偶发返回 200 + 空 data（如 /v2/user/ 的 User=null）——
                //    重试前拦截，避免空 data 被 HTTP 缓存写死导致"找不到用户"。仅检查 /v2/user/ 端点。
                pipeline.AddRetry(new RetryStrategyOptions<HttpResponseMessage> {
                    ShouldHandle = args => ValueTask.FromResult(
                        args.Outcome.Result is { StatusCode: HttpStatusCode.OK } r && HasEmptyUserData(r)),
                    MaxRetryAttempts = 2,
                    DelayGenerator = _ => new ValueTask<TimeSpan?>(TimeSpan.FromMilliseconds(500)),
                    OnRetry = args => {
                        var url = args.Outcome.Result?.RequestMessage?.RequestUri?.AbsoluteUri ?? "?";
                        Log.Warning("空 data 重试第 {Attempt} 次: {Url}", args.AttemptNumber, url);
                        return default;
                    },
                });
            });
        appBuilder.Services.AddSingleton(sp => {
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("wfm");
            return new WarframeMarketClient(http);
        });
        // 语言包下载（独立 HttpClient，不走 wfm 的缓存/限流管道）
        appBuilder.Services.AddSingleton<ILocalizationDownloadService>(_ => new LocalizationDownloadService(new HttpClient { Timeout = TimeSpan.FromSeconds(30) }));
        appBuilder.Services.AddSingleton<IExternalLinkService, ExternalLinkService>();

        // 应用服务（库层领域服务 + GUI 状态/配置）
        appBuilder.Services.AddSingleton<IAppStateService, AppState>();
        appBuilder.Services.AddSingleton<IConfigService, ConfigService>();
        appBuilder.Services.AddSingleton<IPersistentStorage, PersistentStorage>();
        appBuilder.Services.AddSingleton<IVersionService, VersionService>();
        appBuilder.Services.AddSingleton<IItemSearchService>(sp => new ItemSearchService(
            sp.GetRequiredService<WarframeMarketClient>(),
            () => sp.GetRequiredService<IConfigService>().LoadAppConfig().DownloadedLanguages,
            new MemoryCache(new MemoryCacheOptions()))); // 统计缓存：独立 MS 内存缓存（不跟 FusionCache 共用）
        appBuilder.Services.AddSingleton<IUserOrderService, UserOrderService>();
        appBuilder.Services.AddSingleton<IArcanePackService, ArcanePackService>();
        appBuilder.Services.AddSingleton<IOrderService, OrderService>();

        appBuilder.Services.AddSerilog();
        Log.Information("服务注册完成");

        appBuilder.RootComponents.Add<Main>("#app");

        var app = appBuilder.Build();

        app.MainBlazorWindow.Window
            .SetTitle("Warframe.Market")
            .SetUseOsDefaultSize(false)
            .SetSize(1200, 800);

        Log.Information("开始 app.Run()");
        app.Run();
        Log.Information("app.Run() 结束");
    }

    /// <summary>
    /// 检查 /v2/user/ 响应是否为 200 + 空 data（API 偶发返回 {"data":null}）。
    /// 读取后重建响应体（流已消耗），供缓存层继续使用。
    /// </summary>
    private static bool HasEmptyUserData(HttpResponseMessage resp)
    {
        if (resp.RequestMessage?.RequestUri is not { } uri
            || !uri.AbsolutePath.StartsWith("/v2/user/", StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            var body = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            resp.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json"); // 重建（流已消耗）
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("data", out var data))
            {
                return data.ValueKind == JsonValueKind.Null
                    || (data.ValueKind == JsonValueKind.Array && data.GetArrayLength() == 0);
            }
        }
        catch (Exception ex)
        {
            // 读 body 失败不重试（避免误伤正常响应）
            Log.Debug("空 data 检查读 body 失败: {Url}", resp.RequestMessage?.RequestUri);
        }

        return false;
    }
}
