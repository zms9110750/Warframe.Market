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

        // Warframe.Market HTTP 客户端：缓存(命中跳过后续策略) → 429 指数重试 → 令牌桶限流 3/s
        // HTTP 响应缓存键 = {method}/{scheme}/{host}{path}，GET 全自动缓存，无需业务层管缓存
        appBuilder.Services.AddHttpClient("wfm", c => c.BaseAddress = new Uri("https://api.warframe.market"))
            .AddResilienceHandler("wfm", (pipeline, ctx) => {
                pipeline.AddCaching(new HttpCachingStrategyOptions {
                    HybridCache = ctx.ServiceProvider.GetRequiredService<HybridCache>(),
                    CacheKeyProvider = CacheConfig.CacheKeyProvider,
                    HybridCacheSetEntryOptionsProvider = (pipelineCtx, _) => new ValueTask<HybridCacheEntryOptions?>(
                        CacheConfig.EntryOptionsProvider(pipelineCtx.GetRequestMessage()?.RequestUri?.LocalPath ?? "")),
                });
                pipeline.AddRetry(new RetryStrategyOptions<HttpResponseMessage> {
                    ShouldHandle = args => ValueTask.FromResult(
                        args.Outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests),
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                });
                pipeline.AddRateLimiter(new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions {
                    PermitLimit = 3,
                    SegmentsPerWindow = 1,
                    Window = TimeSpan.FromSeconds(1),
                    QueueLimit = 600,
                }));
            });
        appBuilder.Services.AddSingleton(sp => {
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("wfm");
            return new WarframeMarketClient(http);
        });

        // Gitee 更新源
        appBuilder.Services.AddRefitClient<IGitee>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://gitee.com/api/v5"));

        // 应用服务（库层领域服务 + GUI 状态/配置）
        appBuilder.Services.AddSingleton<AppState>();
        appBuilder.Services.AddSingleton<ConfigService>();
        appBuilder.Services.AddSingleton<PersistentStorage>();
        appBuilder.Services.AddSingleton<UpdateService>();
        appBuilder.Services.AddSingleton<IItemSearchService, ItemSearchService>();
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
