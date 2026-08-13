using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Orders;
using zms9110750.WarframeMarketApi.Models.Users;
using zms9110750.Warframe.Market.GUI.Services;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>
/// OrderService：打开子面板（订单加载+筛选）与点击排序（列定义）的核心逻辑单测
/// </summary>
public class OrderServiceTests
{
    private static Order MakeOrder(string type, string status, int? rank, string id = "o1", int price = 100, int quantity = 1)
    {
        return new Order(id, type, price, quantity, 1, null, rank, null, null, null, true,
            "2026-08-01T00:00:00Z", "2026-08-01T00:00:00Z", "item1", null,
            new UserShort(id + "u", $"User{id}", $"user{id}", null, 10, "pc", false, "en", status, null, "2026-08-01T00:00:00Z"));
    }

    private static OrderService CreateService()
    {
        return new(new WarframeMarketClient(new HttpClient(new FakeHttpMessageHandler()) {
            BaseAddress = new Uri("https://api.warframe.market")
        }));
    }

    // ─── 筛选（打开子面板后的本地过滤） ───

    [Fact]
    public void FilterOrders_buy_vs_sell()
    {
        var svc = CreateService();
        var orders = new[] { MakeOrder("sell", "ingame", 0), MakeOrder("buy", "ingame", 0) };

        Assert.Single(svc.FilterOrders(orders, showBuy: false, "all", 0, 0));
        Assert.Single(svc.FilterOrders(orders, showBuy: true, "all", 0, 0));
        Assert.Equal("buy", svc.FilterOrders(orders, showBuy: true, "all", 0, 0).First().Type);
    }

    [Fact]
    public void FilterOrders_user_status()
    {
        var svc = CreateService();
        var orders = new[] { MakeOrder("sell", "online", 0), MakeOrder("sell", "ingame", 0), MakeOrder("sell", "offline", 0) };

        Assert.Equal(2, svc.FilterOrders(orders, false, "online", 0, 0).Count());
        Assert.Single(svc.FilterOrders(orders, false, "ingame", 0, 0));
        Assert.Equal(3, svc.FilterOrders(orders, false, "all", 0, 0).Count());
    }

    [Fact]
    public void FilterOrders_rank_buy_ge_sell_le()
    {
        var svc = CreateService();
        var orders = new[]
        {
            MakeOrder("sell", "all", 0, "s1"),
            MakeOrder("sell", "all", 3, "s2"),
            MakeOrder("sell", "all", 5, "s3"),
            MakeOrder("buy", "all", 3, "b1"),
            MakeOrder("buy", "all", 5, "b2"),
        };

        // 滑块 = 买家想要 N 级：售订单显示 N 级及以上（Rank >= 3 → s2,s3）
        Assert.Equal(2, svc.FilterOrders(orders, false, "all", 3, 5).Count());
        // 购订单显示 N 级及以下（Rank <= 3 → b1）
        Assert.Single(svc.FilterOrders(orders, true, "all", 3, 5));
        // 无等级滑块（selectedRank=0）：不过滤
        Assert.Equal(3, svc.FilterOrders(orders, false, "all", 0, 5).Count());
    }

    [Fact]
    public void FilterOrders_price_range_and_min_quantity()
    {
        var svc = CreateService();
        var orders = new[]
        {
            MakeOrder("sell", "all", 0, "p50", price: 50),
            MakeOrder("sell", "all", 0, "p150", price: 150),
            MakeOrder("sell", "all", 0, "p300", price: 300, quantity: 3),
        };

        // 最低价 100
        Assert.Equal(2, svc.FilterOrders(orders, false, "all", 0, 0, minPrice: 100).Count());
        // 价格区间 100~200
        Assert.Single(svc.FilterOrders(orders, false, "all", 0, 0, minPrice: 100, maxPrice: 200));
        // 最少数量 2（p300 数量 3）
        Assert.Single(svc.FilterOrders(orders, false, "all", 0, 0, minQuantity: 2));
        // 组合：价格 >= 100 且数量 >= 2 → p300
        Assert.Single(svc.FilterOrders(orders, false, "all", 0, 0, minPrice: 100, minQuantity: 2));
    }

    // ─── 订单加载（展开子面板的数据源） ───

    [Fact]
    public async Task GetOrdersAsync_returns_orders()
    {
        var handler = new FakeHttpMessageHandler();
        var slug = "secura_dual_cestra";
        handler.Map($"/v2/orders/item/{slug}", Data.File("orders", "orders-item.json"));
        var svc = new OrderService(new WarframeMarketClient(new HttpClient(handler) {
            BaseAddress = new Uri("https://api.warframe.market")
        }));

        var orders = await svc.GetOrdersAsync(slug);

        Assert.NotEmpty(orders);
        Assert.False(string.IsNullOrEmpty(orders[0].Id));
    }

    // ─── 列定义（点击排序的配置） ───

    private static ItemShort MakeItem(ItemSubtypeSet? subtypes, HashSet<string>? tags = null)
    {
        return new ItemShort("id", "slug", "GameRef", tags ?? new(), 5, null, null, null, null, null, null, subtypes,
            new Dictionary<Language, LanguagePake>());
    }

    [Fact]
    public void BuildColumns_normal_item()
    {
        var svc = CreateService();
        var columns = svc.BuildColumns(MakeItem(null));

        Assert.Contains(columns, c => c.Text == "价格" && c.Sortable && c.RightAlign);
        Assert.Contains(columns, c => c.Text == "联系" && !c.Sortable); // 联系列不可排序
        Assert.DoesNotContain(columns, c => c.Text == "等级"); // 普通物品无等级列
    }

    [Fact]
    public void BuildColumns_mod_item_has_rank_column()
    {
        var svc = CreateService();
        var subtypes = new ItemSubtypeSet { "mod" }; // IsMod = Overlaps(mod keywords)
        var columns = svc.BuildColumns(MakeItem(subtypes));

        Assert.Contains(columns, c => c.Text == "等级" && c.Value == nameof(Order.Rank) && c.Sortable);
    }

    [Fact]
    public void BuildColumns_ayatan_item_has_star_columns()
    {
        var svc = CreateService();
        var columns = svc.BuildColumns(MakeItem(new ItemSubtypeSet { "ayatan_sculpture" })); // IsAyatan

        Assert.Contains(columns, c => c.Text == "琥珀星" && c.Value == nameof(Order.AmberStars));
        Assert.Contains(columns, c => c.Text == "青蓝星" && c.Value == nameof(Order.CyanStars));
    }
}
