using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Orders;

namespace zms9110750.WarframeMarketApi.Services;

/// <summary>订单表列定义（不依赖 UI 框架，GUI 转 DataTableHeader；排序点击按 Sortable/Value 生效）</summary>
public record OrderColumn(
    string Text,
    string Value,
    bool Sortable = true,
    bool RightAlign = false);

/// <summary>
/// 订单服务：拉取物品全量订单 + 本地筛选（购/售、用户状态、等级）+ 动态列定义。
/// UI 的"展开子面板"（OrderTop 初始化）与"点击排序"（列配置）都走此接口，可脱离 UI 单测。
/// </summary>
public interface IOrderService
{
    /// <summary>拉取物品全部订单（HTTP 实时，不做缓存）</summary>
    Task<Order[]> GetOrdersAsync(string slug, CancellationToken ct = default);

    /// <summary>本地筛选：购/售 + 用户状态（all/online/ingame）+ 等级（购 Rank&gt;=N、售 Rank&lt;=N）</summary>
    IEnumerable<Order> FilterOrders(IEnumerable<Order> orders, bool showBuy, string userStatus, int selectedRank, int maxRank,
        int minPrice = 0, int maxPrice = 0, int minQuantity = 0);

    /// <summary>按物品子类型构建订单表列定义（mod/arcane→等级、ayatan→星级、component/relic/riven→类型）</summary>
    OrderColumn[] BuildColumns(ItemShort item);
}
