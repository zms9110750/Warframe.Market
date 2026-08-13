using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Orders;

namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>
/// 订单服务实现：订单全量拉取 + 纯函数筛选 + 动态列定义
/// </summary>
public class OrderService : IOrderService
{
    private readonly WarframeMarketClient _wfm;

    public OrderService(WarframeMarketClient wfm)
    {
        _wfm = wfm;
    }

    public async Task<Order[]> GetOrdersAsync(string slug, CancellationToken ct = default)
    {
        var resp = await _wfm.GetOrdersItemAsync(slug, ct);
        return resp?.Content?.Data ?? [];
    }

    public IEnumerable<Order> FilterOrders(
        IEnumerable<Order> orders, bool showBuy, string userStatus, int selectedRank, int maxRank,
        int minPrice = 0, int maxPrice = 0, int minQuantity = 0)
    {
        var q = orders.Where(o => showBuy ? (o.Type is "buy" or "Buy") : (o.Type is "sell" or "Sell"));
        if (userStatus == "online")
        {
            q = q.Where(o => o.User?.Status is "online" or "ingame");
        }
        else if (userStatus == "ingame")
        {
            q = q.Where(o => o.User?.Status == "ingame");
        }

        if (maxRank > 0 && selectedRank > 0)
        {
            // 滑块语义：买家设 N 级 → 售订单显示 N 级及以上（买高级卡）；购订单显示 N 级及以下（卖低价卡）
            if (showBuy)
            {
                q = q.Where(o => (o.Rank ?? 0) <= selectedRank);
            }
            else
            {
                q = q.Where(o => (o.Rank ?? 0) >= selectedRank);
            }
        }

        // 金额区间：minPrice>0 表示下限，maxPrice>0 表示上限（0 = 不限）
        if (minPrice > 0)
        {
            q = q.Where(o => o.Platinum >= minPrice);
        }

        if (maxPrice > 0)
        {
            q = q.Where(o => o.Platinum <= maxPrice);
        }
        // 最小数量（无上限）
        if (minQuantity > 0)
        {
            q = q.Where(o => o.Quantity >= minQuantity);
        }

        return q;
    }

    public OrderColumn[] BuildColumns(ItemShort item)
    {
        var columns = new List<OrderColumn>
        {
            new("联系", nameof(Order.Id), Sortable: false),
            new("卖家", nameof(Order.User)),
            new("声誉", nameof(Order.User)),
            new("价格", nameof(Order.Platinum), RightAlign: true),
            new("数量", nameof(Order.Quantity)),
        };

        var s = item.Subtypes ?? FallbackSubtypes(item.Tags);
        if (s is { IsMod: true } or { IsArcane: true })
        {
            columns.Add(new OrderColumn("等级", nameof(Order.Rank)));
        }
        else if (s is { IsAyatan: true })
        {
            columns.Add(new OrderColumn("琥珀星", nameof(Order.AmberStars)));
            columns.Add(new OrderColumn("青蓝星", nameof(Order.CyanStars)));
        }
        else if (s is { IsComponent: true } or { IsRelic: true } or { IsRiven: true })
        {
            columns.Add(new OrderColumn("类型", nameof(Order.Subtype)));
        }

        return columns.ToArray();
    }

    private static ItemSubtypeSet? FallbackSubtypes(HashSet<string>? tags)
    {
        if (tags == null)
        {
            return null;
        }

        var result = new ItemSubtypeSet();
        foreach (var t in tags)
        {
            result.Add(t);
        }

        return result;
    }
}
