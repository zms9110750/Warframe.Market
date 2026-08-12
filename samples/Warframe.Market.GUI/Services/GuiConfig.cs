namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>
/// GUI 全局配置（集中可改；DI 注册单例，Program/服务/页面经注入获取）。
/// 值保持与既有硬编码一致——仅收敛改动点，不改变行为。
/// 注意：与 Data/AppConfig（用户默认配置：语言/平台/语言包）是不同概念。
/// </summary>
public class GuiConfig
{
    /// <summary>Warframe.Market API 基址</summary>
    public string ApiBaseAddress { get; init; } = "https://api.warframe.market";

    /// <summary>SQLite 分布式缓存文件名（程序目录）</summary>
    public string CacheDbFile { get; init; } = "cache.db";

    /// <summary>并发限流：每窗口许可数（3/s）</summary>
    public int RateLimitPermit { get; init; } = 3;

    /// <summary>并发限流：排队上限</summary>
    public int RateLimitQueue { get; init; } = 600;

    /// <summary>限流重试（本地队列满）：最大次数</summary>
    public int RateLimitRetryMax { get; init; } = 5;

    /// <summary>限流重试：按当前排队数估算的等待（ms/个）</summary>
    public int RateLimitRetryPerQueuedMs { get; init; } = 333;

    /// <summary>限流重试：基础等待（ms）</summary>
    public int RateLimitRetryBaseMs { get; init; } = 300;

    /// <summary>429 重试（服务器限流）：最大次数</summary>
    public int Http429RetryMax { get; init; } = 3;

    /// <summary>429 重试：固定等待（ms）</summary>
    public int Http429RetryDelayMs { get; init; } = 1000;

    /// <summary>HTTP 响应缓存：其他端点默认 TTL</summary>
    public TimeSpan CacheDefaultTtl { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>HTTP 响应缓存：用户信息 TTL（仅内存）</summary>
    public TimeSpan CacheUserTtl { get; init; } = TimeSpan.FromMinutes(10);
}
