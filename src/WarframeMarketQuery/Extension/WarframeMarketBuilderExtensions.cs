using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.RateLimiting;
using Polly.Retry;
using Refit;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.RateLimiting;
using WarframeMarketQuery.API;
using WarframeMarketQuery.Arcane;
using WarframeMarketQuery.Extension;
using WarframeMarketQuery.Model;
using WarframeMarketQuery.Model.Items;
using ZiggyCreatures.Caching.Fusion;
using zms9110750.TreeCollection.Trie;

namespace WarframeMarketQuery.Extension;

/// <summary>
/// 依赖注入扩展
/// </summary>
public static class WarframeMarketBuilderExtensions
{
    /// <summary>
    /// 注册WarframeMarketClient
    /// </summary>
    /// <param name="services">服务容器</param>
    /// <param name="language"><see cref="WarframeMarketClient"/>的语言设置</param>
    /// <param name="queueSec">允许排队多久</param>
    /// <param name="permitLimit">每秒的许可数</param>
    /// <param name="cachePath">缓存路径</param>
    /// <returns></returns>
    public static IServiceCollection AddWarframeMarketClient(this IServiceCollection services, Language language = Language.ZhHans, int queueSec = 6, double permitLimit = 4)
    {
        var assembly = Assembly.GetExecutingAssembly().GetName();
        services.AddHttpClient<WarframeMarketApi>(client =>
        {
            client.BaseAddress = new Uri("https://api.warframe.market");
            client.DefaultRequestHeaders.Add("Language", JsonNamingPolicy.KebabCaseLower.ConvertName(language.ToString()));
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"{assembly.Name}/{assembly.Version} (https://github.com/zms9110750)");
        })
        .AddRefitClient<IWarframeMarketApi>(settingsAction: provider => new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(IWarframeMarketApi.V2options),
            UrlParameterKeyFormatter = new CamelCaseUrlParameterKeyFormatter()
        })
        .AddRefitClient<IWarframeMarketApiV1>(settingsAction: provider => new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(IWarframeMarketApiV1.V1options)
        })
        .AddResilienceHandler("wm", builder =>
        {
            builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                   .HandleResult(response => response.StatusCode == HttpStatusCode.TooManyRequests),
                UseJitter = true,
                BackoffType = DelayBackoffType.Exponential
            })
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>().Handle<RateLimiterRejectedException>(),
                MaxRetryAttempts = int.MaxValue,
                Delay = TimeSpan.FromSeconds(queueSec / permitLimit)
            })
            .AddRateLimiter(new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = 3,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1 / permitLimit),
                TokensPerPeriod = 1,
                QueueLimit = queueSec
            }));
        });
        services.AddSingleton<WarframeMarketApi>();
        return services;
    }

    /// <summary>
    /// 将 Program.cs 中 “剩余部分” 的依赖注入集中成一键注册方法。
    /// 该方法不修改或扩展 <see cref="AddWarframeMarketClient"/> 的签名（避免干扰已有配置）。
    /// </summary>
    /// <param name="services">服务容器</param>
    /// <param name="yamlConfigPath">YAML 配置路径，默认 "赋能包配置.yaml"</param>
    /// <returns>返回同一 IServiceCollection 以便链式调用</returns>
    public static IServiceCollection AddWarframeMarketProgramServices(this IServiceCollection services, string yamlConfigPath = "赋能包配置.yaml")
    {
        // 加载 YAML 配置（与原 Program.cs 行为一致）
        services.AddSingleton<IConfiguration>(  sp => new ConfigurationBuilder().AddYamlFile(yamlConfigPath).Build());
        
        // 从配置节读取 ArcanePack[]
        services.AddSingleton(sp => sp.GetRequiredService<IConfiguration>().GetSection("赋能包配置").Get<ArcanePack[]>()!);

        // 构建并填充 Trie（如果 HybridCache 中存在索引则使用）
        services.AddSingleton(sp =>
        {
            Trie trie = new Trie(['_', ' ', '·']);
            var hybridCache = sp.GetService<IFusionCache>();
            var index = hybridCache?.GetOrDefault<string[]>(nameof(Trie).ToLower());
            foreach (var item in index ?? Array.Empty<string>())
            {
                trie.Add(item);
            }
            return trie;
        });

        return services;
    }

    public static IHttpClientBuilder AddRefitClient<T>(this IHttpClientBuilder clientBuilder, Func<IServiceProvider, RefitSettings?>? settingsAction) where T : class
    {
        return clientBuilder.Services.AddRefitClient<T>(settingsAction, clientBuilder.Name);
    }
}