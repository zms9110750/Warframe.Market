using System.Text;
using Axion.Extensions.Polly.Caching.Hybrid;

namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>
/// HTTP 响应缓存的外部可配置设置（唯一配置点：Program.cs 的 AddCaching 从这里取）。
/// 缓存键 = {method}/{scheme}/{host}{path}{query}{|lang=语言头} —— 语言参数（i18n body）纳入键。
/// 各端点缓存策略（按 LocalPath 分类）可整体替换 EntryOptionsProvider。
/// </summary>
public static class CacheConfig
{
    /// <summary>其他端点的默认缓存有效期（半分钟）</summary>
    public static TimeSpan Ttl { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>缓存键生成器（可整体替换）</summary>
    public static ValueTask<string> CacheKeyProvider(Polly.ResilienceContext context)
    {
        var message = context.GetRequestMessage() ?? throw new InvalidOperationException();
        var uri = message.RequestUri ?? throw new InvalidOperationException();

        var key = new StringBuilder(message.Method.Method.ToLowerInvariant()).Append('/');

        if (uri.IsAbsoluteUri)
        {
            key.Append(uri.Scheme).Append('/').Append(uri.IdnHost);
            if (!uri.IsDefaultPort)
            {
                key.Append(':').Append(uri.Port);
            }
        }
        else
        {
            key.Append('-');
        }

        if (uri.LocalPath.Length > 1)
        {
            key.Append(uri.LocalPath);
        }

        // query 隔离（避免 rankLt=5 / rankLt=3 串缓存）
        key.Append(uri.Query);

        // Language 请求头（i18n body 随语言变化）纳入键
        if (message.Headers.TryGetValues("Language", out var langs))
        {
            key.Append("|lang=").Append(string.Join(",", langs));
        }

        return new ValueTask<string>(key.ToString());
    }

    /// <summary>按请求路径返回缓存条目选项（可整体替换为自定义策略）</summary>
    public static Func<string, Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions?> EntryOptionsProvider { get; set; } = DefaultEntryOptions;

    private static Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions? DefaultEntryOptions(string localPath)
    {
        // versions：无限 TTL + 仅内存
        if (localPath == "/v2/versions")
        {
            return new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions {
                Flags = Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryFlags.DisableDistributedCache,
            };
        }

        // user：10 分钟 TTL + 仅内存（无限缓存会因陈旧/异常响应导致"找不到"，刷新按钮可强制清缓存重查）
        if (localPath.StartsWith("/v2/user/", StringComparison.Ordinal))
        {
            return new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions {
                Expiration = TimeSpan.FromMinutes(10),
                Flags = Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryFlags.DisableDistributedCache,
            };
        }

        // items：无限 TTL，带硬盘缓存（落 Sqlite 分布式层），语言参数已在缓存键中隔离
        if (localPath == "/v2/items")
        {
            return new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions();
        }

        // V1 统计：持续到下一个 UTC 0（每个请求当场计算剩余时长）
        if (localPath.StartsWith("/v1/", StringComparison.Ordinal)
            && localPath.EndsWith("/statistics", StringComparison.Ordinal))
        {
            return new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions {
                Expiration = TimeUntilNextUtcMidnight(),
            };
        }

        // 订单数据要求实时：禁用缓存写（读端永远 miss，每次都走网络）
        if (localPath.StartsWith("/v2/orders/", StringComparison.Ordinal))
        {
            return new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions {
                Flags = Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryFlags.DisableLocalCacheWrite
                      | Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryFlags.DisableDistributedCacheWrite,
            };
        }

        // 其他：默认 TTL
        return new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions {
            Expiration = Ttl,
        };
    }

    /// <summary>到下一个 UTC 0 的剩余时长（当场计算）</summary>
    public static TimeSpan TimeUntilNextUtcMidnight()
    {
        var now = DateTime.UtcNow;
        return now.Date.AddDays(1) - now;
    }
}
