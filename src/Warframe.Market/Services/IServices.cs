using zms9110750.WarframeMarketApi.Models.Arcane;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Orders;
using zms9110750.WarframeMarketApi.Models.Statistics;
using zms9110750.WarframeMarketApi.Models.Users;

namespace zms9110750.WarframeMarketApi.Services;

/// <summary>用户搜索结果（UserOrderService.SearchUserAsync 的返回值）</summary>
public class UserSearchResult
{
    public bool NotFound { get; set; }
    public bool Loading { get; set; }
    public bool LoadingPrices { get; set; }
    public string? Error { get; set; }
    public User? User { get; set; }
    public List<Order>? Orders { get; set; }
    public Dictionary<string, ItemShort?> ItemCache { get; set; } = new();
    public Dictionary<string, Statistic?> Prices { get; set; } = new();
}

/// <summary>
/// 物品搜索服务：内存 Trie 模糊搜索（索引来自 items 列表）+ 统计获取 + 参考价。
/// UI 的搜索触发只调用此接口，实现可脱离 UI 单测。
/// </summary>
public interface IItemSearchService
{
    /// <summary>模糊搜索（/ 分隔多词，Trie 段匹配）</summary>
    Task<List<ItemShort>> SearchAsync(string query, CancellationToken ct = default);

    /// <summary>按 slug / id / 本地化名（含归一化）精确查物品</summary>
    Task<ItemShort?> FindByKeyAsync(string key);

    /// <summary>获取物品统计（V1，HTTP 缓存）</summary>
    Task<Statistic?> GetStatisticAsync(string slug, CancellationToken ct = default);

    /// <summary>基础参考价</summary>
    double? GetReferencePrice(Statistic? stat);

    /// <summary>满级参考价</summary>
    double? GetMaxReferencePrice(Statistic? stat);

    /// <summary>清空内存索引（强制刷新用）</summary>
    void Invalidate();
}

/// <summary>用户订单查询服务：确认用户存在 → 拉订单 → 补物品 → 加载价格</summary>
public interface IUserOrderService
{
    Task<UserSearchResult> SearchUserAsync(string name, CancellationToken ct = default);
}

/// <summary>赋能包期望值计算服务</summary>
public interface IArcanePackService
{
    /// <summary>计算赋能包期望价值（purchase=每日购买组数，0=自用）</summary>
    Task<double> GetReferencePriceAsync(ArcanePackConfig pack, int purchase = 0);

    /// <summary>物品日均交易量（90 天，按合成消耗折算）</summary>
    double GetDailyVolume(Statistic? stat);
}
