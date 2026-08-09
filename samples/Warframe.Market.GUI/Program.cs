using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Http.Resilience;
using Polly.RateLimiting;
using Refit;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Services;
using zms9110750.Warframe.Market.GUI.Api;
using zms9110750.Warframe.Market.GUI.Services;

namespace zms9110750.Warframe.Market.GUI;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // Serilog 最先初始化：任何阶段的日志（含启动早期异常）都能落盘
        Log.Logger = new LoggerConfiguration()
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

        appBuilder.Services.AddMasaBlazor();
        Log.Information("AddMasaBlazor 完成");

        // FusionCache（Sqlite 分布式持久化）→ HybridCache 桥接，供下方 HTTP 缓存使用
        appBuilder.Services.AddSqliteCache("cache.db")
            .AddFusionCache()
            .WithRegisteredDistributedCache()
            .WithSerializer(new FusionCacheSystemTextJsonSerializer(new JsonSerializerOptions {
                Converters = { new HttpResponseMessageJsonConverter() },
            }))
            .AsHybridCache();

        // Warframe.Market HTTP 客户端：缓存(命中跳过后续策略) → 限流感知重试(本地限流异常/429) → 令牌桶限流 3/s
        // HTTP 响应缓存键 = {method}/{scheme}/{host}{path}，GET 全自动缓存，无需业务层管缓存
        appBuilder.Services.AddHttpClient("wfm", c => c.BaseAddress = new Uri("https://api.warframe.market"))
            .AddResilienceHandler("wfm", (pipeline, ctx) => {
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

                // 共享限流器：retry 读取其队列数估算等待时间
                var limiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions {
                    PermitLimit = 3,
                    SegmentsPerWindow = 1,
                    Window = TimeSpan.FromSeconds(1),
                    QueueLimit = 600,
                });

                // 限流感知重试（内层限流抛 RateLimiterRejectedException 或服务器 429 时重试）：
                // 等待时间 = 当前限流队列中请求数 × 每请求所需时间（3/s → ~333ms）+ 余量
                pipeline.AddRetry(new RetryStrategyOptions<HttpResponseMessage> {
                    ShouldHandle = args => ValueTask.FromResult(
                        args.Outcome.Exception is RateLimiterRejectedException
                        || args.Outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests),
                    MaxRetryAttempts = 3,
                    DelayGenerator = _ => new ValueTask<TimeSpan?>(TimeSpan.FromMilliseconds(((limiter.GetStatistics()?.CurrentQueuedCount ?? 0) * 333) + 300)),
                });

                pipeline.AddRateLimiter(limiter);
            });
        appBuilder.Services.AddSingleton(sp => {
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("wfm");
            return new WarframeMarketClient(http);
        });
        // 语言包下载（独立 HttpClient，不走 wfm 的缓存/限流管道）
        appBuilder.Services.AddSingleton<LocalizationDownloadService>(_ => new(new HttpClient()));

        // Gitee 更新源
        appBuilder.Services.AddRefitClient<IGitee>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://gitee.com/api/v5"));

        // 应用服务（库层领域服务 + GUI 状态/配置）
        appBuilder.Services.AddSingleton<AppState>();
        appBuilder.Services.AddSingleton<ConfigService>();
        appBuilder.Services.AddSingleton<PersistentStorage>();
        appBuilder.Services.AddSingleton<UpdateService>();
        appBuilder.Services.AddSingleton<IItemSearchService>(sp => new ItemSearchService(
            sp.GetRequiredService<WarframeMarketClient>(),
            () => sp.GetRequiredService<ConfigService>().LoadAppConfig().DownloadedLanguages));
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
}
