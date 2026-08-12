namespace zms9110750.WarframeMarketApi.Models.Orders;

/// <summary>
/// 在线玩家中某物品的 Top5 买单和 Top5 卖单
/// </summary>
/// <param name="Sell">Top5 卖单（从买家视角排序）</param>
/// <param name="Buy">Top5 买单（从卖家视角排序）</param>
public record OrderTop(
    Order[] Sell,
    Order[] Buy
);
