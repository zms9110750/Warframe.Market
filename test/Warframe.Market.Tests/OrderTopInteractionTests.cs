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
    private sealed class FakeExternalLink : zms9110750.Warframe.Market.GUI.Services.IExternalLinkService
    {
        public void Open(string url) { }
    }

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

    private static ItemShort MakeAyatanItem()
    {
        // Orta 塑像（目录内）：滑块范围 650~2700，列显示豆子（不显示星数）
        return new ItemShort("id", "ayatan_orta_sculpture", "GameRef", new HashSet<string> { "ayatan_sculpture" }, 0,
            null, null, null, null, null, null, null,
            new Dictionary<Language, LanguagePake>());
    }

    private static Order MakeAyatanOrder(string id, int amber, int cyan, int? perTrade, int price, string type = "sell", string status = "online")
    {
        return new Order(id, type, price, perTrade ?? 1, perTrade ?? 1, null, null, null, amber, cyan, true,
            "2026-08-01T00:00:00Z", "2026-08-01T00:00:00Z", "item1", null,
            new UserShort(id + "u", $"User{id}", $"user{id}", null, 10, "pc", false, "en", status, null, "2026-08-01T00:00:00Z"));
    }

    private static BunitContext CreateCtx(Order[] orders)
    {
        var ctx = new BunitContext();
        ctx.Services.AddMasaBlazor();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddSingleton<zms9110750.Warframe.Market.GUI.Services.IAppStateService>(_ => new AppState(new WarframeMarketClient(new HttpClient())));
        ctx.Services.AddSingleton<zms9110750.Warframe.Market.GUI.Services.IExternalLinkService>(_ => new FakeExternalLink());
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

    [Fact]
    public async Task Ayatan_renders_endo_slider_with_min_max()
    {
        var orders = new[]
        {
            MakeAyatanOrder("1", 1, 3, 1, 100), // 满星 → 2700
            MakeAyatanOrder("2", 0, 0, 6, 50),  // 无星 → 650
        };
        await using var ctx = CreateCtx(orders);
        var cut = ctx.Render<OrderTopPanel>(p => p.Add(m => m.TargetItem, MakeAyatanItem()));

        // 塑像：豆子滑块 min=无星豆子 650，max=满星豆子 2700（不是等级 0~10）
        var slider = cut.Find("input[type=range]");
        Assert.Equal("650", slider.GetAttribute("min"));
        Assert.Equal("2700", slider.GetAttribute("max"));
    }

    [Fact]
    public async Task Ayatan_shows_endo_and_bulk_columns()
    {
        var orders = new[]
        {
            MakeAyatanOrder("1", 1, 3, 1, 100), // 满星 → 豆子 2700
            MakeAyatanOrder("2", 0, 0, 6, 50),  // 无星 → 豆子 650，批量 perTrade=6
        };
        await using var ctx = CreateCtx(orders);
        var cut = ctx.Render<OrderTopPanel>(p => p.Add(m => m.TargetItem, MakeAyatanItem()));

        var text = cut.Markup;
        Assert.Contains("2700", text);     // 满星订单显示豆子
        Assert.Contains("650", text);      // 无星订单显示豆子
        Assert.Contains("批量×6", text);   // 批量列（perTrade>1）
        Assert.DoesNotContain("琥珀星", text); // 不再显示星数列
    }

    private static ItemShort MakeRelicItem()
    {
        return new ItemShort("id", "requiem_iv_relic", "GameRef", new HashSet<string> { "relic" }, 0,
            null, null, null, null, null, null, null,
            new Dictionary<Language, LanguagePake>());
    }

    private static Order MakeRelicOrder(string id, string subtype, int price, string type = "sell")
    {
        return new Order(id, type, price, 1, 1, subtype, null, null, null, null, true,
            "2026-08-01T00:00:00Z", "2026-08-01T00:00:00Z", "item1", null,
            new UserShort(id + "u", $"User{id}", $"user{id}", null, 10, "pc", false, "en", "online", null, "2026-08-01T00:00:00Z"));
    }

    [Fact]
    public async Task Relic_slider_filters_like_mod_rank()
    {
        // 语义同 mod 等级：购显示精炼度 <= 档位；售显示 >= 档位
        var orders = new[]
        {
            MakeRelicOrder("1", "intact", 5),
            MakeRelicOrder("2", "radiant", 20),
        };
        await using var ctx = CreateCtx(orders);
        var cut = ctx.Render<OrderTopPanel>(p => p.Add(m => m.TargetItem, MakeRelicItem()));

        // 遗物：精炼度滑块 0~3（完整→光辉），标签为官方中文
        var slider = cut.Find("input[type=range]");
        Assert.Equal("0", slider.GetAttribute("min"));
        Assert.Equal("3", slider.GetAttribute("max"));

        // 默认售 + 档位 0（完整）：售显示 >=0 = 全部（intact + radiant 都显示）
        Assert.Contains("完整", cut.Markup);
        Assert.Equal(2, CountDataRows(cut));

        // 滑到 3（光辉）：售显示 >=3 = 只剩 radiant
        slider.Change("3");
        Assert.Equal(1, CountDataRows(cut));
        Assert.Contains("光辉", cut.Markup);

        // 购：显示精炼度 <= 档位（买家求购低档，自己可提供更好）——用 buy 订单单独渲染
        var buyOrders = new[]
        {
            MakeRelicOrder("1b", "intact", 5, "buy"),
            MakeRelicOrder("2b", "radiant", 20, "buy"),
        };
        await using var ctxBuy = CreateCtx(buyOrders);
        var cutBuy = ctxBuy.Render<OrderTopPanel>(p => p.Add(m => m.TargetItem, MakeRelicItem()));
        cutBuy.FindAll("button").First(b => b.TextContent.Contains("购")).Click();

        // 购 + 档位 3：购显示 <=3 = 全部
        cutBuy.Find("input[type=range]").Change("3");
        Assert.Equal(2, CountDataRows(cutBuy));

        // 购 + 档位 0：购显示 <=0 = 只剩 intact
        cutBuy.Find("input[type=range]").Change("0");
        Assert.Equal(1, CountDataRows(cutBuy));
    }
}
