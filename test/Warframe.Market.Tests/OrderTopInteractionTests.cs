using Bunit;
using Masa.Blazor;
using Xunit;
using Xunit.Abstractions;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Orders;
using zms9110750.WarframeMarketApi.Models.Users;
using zms9110750.WarframeMarketApi.Services;
using zms9110750.Warframe.Market.GUI.Services;
using OrderTopPanel = zms9110750.Warframe.Market.GUI.Pages.OrderTop;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>
/// OrderTop 子面板交互测试（不开 UI，bUnit 直接驱动组件事件）：
/// 1. 滑块等级筛选（onchange → FilterOrders）
/// 2. 表头点击排序（Masa MDataTable OnSort）
/// 用真 OrderService（FilterOrders 纯函数）验证 GUI 行为与库逻辑一致。
/// </summary>
public class OrderTopInteractionTests
{
    private readonly ITestOutputHelper _output;

    public OrderTopInteractionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static Order MakeOrder(string type, string status, int? rank, int price, string id)
    {
        return new Order(id, type, price, 1, 1, null, rank, null, null, null, true,
            "2026-08-01T00:00:00Z", "2026-08-01T00:00:00Z", "item1", null,
            new UserShort(id + "u", $"User{id}", $"user{id}", null, 10, "pc", false, "en", status, null, "2026-08-01T00:00:00Z"));
    }

    private static ItemShort MakeModItem()
    {
        // MaxRank=10 的 mod：滑块 min=0 max=10，且带"等级"列
        return new ItemShort("id", "blind_rage", "GameRef", new HashSet<string>(), 10,
            null, null, null, null, null, null, new ItemSubtypeSet { "mod" },
            new Dictionary<Language, LanguagePake>());
    }

    private static BunitContext CreateCtx(Order[] orders)
    {
        var ctx = new BunitContext();
        ctx.Services.AddMasaBlazor();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddSingleton<AppState>(_ => new AppState(new WarframeMarketClient(new HttpClient())));
        ctx.Services.AddSingleton<IOrderService>(new FakeOrderService(orders));
        return ctx;
    }

    private static IRenderedComponent<OrderTopPanel> RenderOrderTop(BunitContext ctx)
    {
        return ctx.Render<OrderTopPanel>(p => p
            .Add(m => m.TargetItem, MakeModItem()));
    }

    [Fact]
    public async Task OrderTop_renders_and_loads_orders()
    {
        await using var ctx = CreateCtx(SellOrders);
        var cut = RenderOrderTop(ctx);

        // 加载完成：等级滑块存在（max=10）
        Assert.NotNull(cut.Find("input[type=range]"));
        Assert.Equal("10", cut.Find("input[type=range]").GetAttribute("max"));
    }

    [Fact]
    public async Task Rank_slider_filters_sell_orders_to_rank_ge_selected()
    {
        // 语义：滑块 = 买家想要 N 级，售订单显示 N 级及以上（修 bug 的核心场景：设 10 级不再出现 0 级）
        await using var ctx = CreateCtx(SellOrders);
        var cut = RenderOrderTop(ctx);
        // 初始 selectedRank=0：不过滤（全部售订单）
        Assert.Equal(3, CountDataRows(cut));

        // 滑块拖到 10 级：只剩 Rank>=10 的售订单
        cut.Find("input[type=range]").Change("10");
        Assert.Equal(1, CountDataRows(cut));
    }

    [Fact]
    public async Task Rank_slider_filters_buy_orders_to_rank_le_selected()
    {
        await using var ctx = CreateCtx(BuyOrders);
        var cut = RenderOrderTop(ctx);

        // 切到"购"
        cut.FindAll("button").First(b => b.TextContent.Contains("购")).Click();
        Assert.Equal(3, CountDataRows(cut));

        // 滑块 5 级：购订单显示 Rank<=5
        cut.Find("input[type=range]").Change("5");
        Assert.Equal(2, CountDataRows(cut));
    }

    [Fact]
    public async Task Header_click_sorts_orders_by_price()
    {
        await using var ctx = CreateCtx(SellOrders);
        var cut = RenderOrderTop(ctx);

        // 默认已按价格升序（SortBy=Platinum, SortDesc=false → 最低价在前）
        Assert.Contains("50", FirstRowText(cut));

        // 需求3：MustSort 已配置（点击表头只在 升序/降序 间切换，不会进入"不排序"）
        var table = cut.FindComponent<Masa.Blazor.MDataTable<Order>>();
        Assert.True(table.Instance.Options.MustSort);
        Assert.Contains(nameof(Order.Platinum), table.Instance.Options.SortBy);
        Assert.False(table.Instance.Options.SortDesc.FirstOrDefault());
    }

    [Fact]
    public async Task Default_sort_sell_ascending_price()
    {
        // 需求1：售默认价格升序（低价在前）
        await using var ctx = CreateCtx(SellOrders);
        var cut = RenderOrderTop(ctx);
        Assert.Contains("50", FirstRowText(cut));
    }

    [Fact]
    public async Task Default_sort_buy_descending_price()
    {
        // 需求1：购默认价格降序（高价在前）
        await using var ctx = CreateCtx(BuyOrders);
        var cut = RenderOrderTop(ctx);
        cut.FindAll("button").First(b => b.TextContent.Contains("购")).Click();
        Assert.Contains("300", FirstRowText(cut));
    }

    [Fact]
    public async Task Slider_filter_keeps_sort()
    {
        // 需求2：调滑块后排序保持（去 @key 验证：筛选后仍是价格升序）
        await using var ctx = CreateCtx(SellOrders);
        var cut = RenderOrderTop(ctx);
        cut.Find("input[type=range]").Change("5");
        Assert.Equal(2, CountDataRows(cut)); // 只剩 s5(150)/s10(300)
        Assert.Contains("150", FirstRowText(cut)); // 升序保持
    }

    [Fact]
    public async Task Price_min_input_filters_orders()
    {
        // 需求4：最低价输入框筛选
        await using var ctx = CreateCtx(SellOrders);
        var cut = RenderOrderTop(ctx);
        cut.FindAll("input[type=number]").First().Change("100"); // 最低价
        Assert.Equal(2, CountDataRows(cut)); // s5(150)/s10(300)
    }

    [Fact]
    public async Task Min_quantity_input_filters_orders()
    {
        // 需求5：最少数量输入框筛选（SellOrders 数量全 1 → 输入 3 后 0 行）
        await using var ctx = CreateCtx(SellOrders);
        var cut = RenderOrderTop(ctx);
        cut.FindAll("input[type=number]").Last().Change("3"); // 最少数量
        Assert.Equal(0, CountDataRows(cut));
    }

    // ─── 假数据 ───

    private static readonly Order[] SellOrders =
    [
        MakeOrder("sell", "ingame", 0, 50, "s0"),
        MakeOrder("sell", "ingame", 5, 150, "s5"),
        MakeOrder("sell", "ingame", 10, 300, "s10"),
    ];

    private static readonly Order[] BuyOrders =
    [
        MakeOrder("buy", "ingame", 0, 50, "b0"),
        MakeOrder("buy", "ingame", 5, 150, "b5"),
        MakeOrder("buy", "ingame", 10, 300, "b10"),
    ];

    /// <summary>假 IOrderService：GetOrdersAsync 返回固定订单；FilterOrders/BuildColumns 委托真 OrderService</summary>
    private sealed class FakeOrderService : IOrderService
    {
        private readonly OrderService _real;

        public FakeOrderService(Order[] orders)
        {
            Orders = orders;
            _real = new OrderService(new WarframeMarketClient(new HttpClient(new FakeHttpMessageHandler()) {
                BaseAddress = new Uri("https://api.warframe.market")
            }));
        }

        public Order[] Orders { get; }

        public Task<Order[]> GetOrdersAsync(string slug, CancellationToken ct = default)
        {
            return Task.FromResult(Orders);
        }

        public IEnumerable<Order> FilterOrders(
            IEnumerable<Order> orders, bool showBuy, string userStatus, int selectedRank, int maxRank,
            int minPrice = 0, int maxPrice = 0, int minQuantity = 0)
        {
            return _real.FilterOrders(orders, showBuy, userStatus, selectedRank, maxRank, minPrice, maxPrice, minQuantity);
        }

        public OrderColumn[] BuildColumns(ItemShort item)
        {
            return _real.BuildColumns(item);
        }
    }

    // ─── 辅助 ───

    /// <summary>数据行数（排除分组行与空数据行）</summary>
    private static int CountDataRows(IRenderedComponent<OrderTopPanel> cut)
    {
        return cut.FindAll("tbody tr:not(.m-data-table__group-header):not(.m-data-table__empty-wrapper)").Count;
    }

    /// <summary>第一行数据文本</summary>
    private static string FirstRowText(IRenderedComponent<OrderTopPanel> cut)
    {
        return cut.FindAll("tbody tr:not(.m-data-table__group-header)").First().TextContent;
    }
}
