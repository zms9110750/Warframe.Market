using Bunit;
using Masa.Blazor;
using Xunit;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Orders;
using zms9110750.WarframeMarketApi.Models.Statistics;
using zms9110750.WarframeMarketApi.Models.Users;
using zms9110750.WarframeMarketApi.Services;
using UserResultTablePanel = zms9110750.Warframe.Market.GUI.Pages.UserSearch.UserResultTable;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>
/// UserResultTable 参考价/差价三态测试：
/// 价格未加载（Prices 无 slug）→ "加载中"；加载完成有值 → 数值；加载完成无数据（stat null）→ "-"
/// </summary>
public class UserResultTableTests
{
    private static Order MakeOrder(string id, string itemId, int price = 100)
    {
        return new Order(id, "sell", price, 1, 1, null, 0, null, null, null, true,
            "2026-08-01T00:00:00Z", "2026-08-01T00:00:00Z", itemId, null,
            new UserShort(id + "u", $"User{id}", $"user{id}", null, 10, "pc", false, "en", "online", null, "2026-08-01T00:00:00Z"));
    }

    private static ItemShort MakeItem(string slug, string itemId)
    {
        return new ItemShort(itemId, slug, "GameRef", new HashSet<string>(), 5,
            null, null, null, null, null, null, null,
            new Dictionary<Language, LanguagePake>());
    }

    private static Statistic MakeStat(double median)
    {
        var entries = Enumerable.Range(1, 7)
            .Select(i => new Entry(
                DateTime.UtcNow.AddDays(-i), 10, 50f, 100f, 75f, 75f, (float)median,
                null, $"id{i}", null, null, null, null, null, null, null, null))
            .ToArray();
        return new Statistic(new Payload(
            new Period(Array.Empty<Entry>(), entries),
            new Period(Array.Empty<Entry>(), Array.Empty<Entry>())));
    }

    private sealed class FakeItemSearch : IItemSearchService
    {
        public Task<List<ItemShort>> SearchAsync(string query, CancellationToken ct = default)
        {
            return Task.FromResult(new List<ItemShort>());
        }

        public Task<ItemShort?> FindByKeyAsync(string key)
        {
            return Task.FromResult<ItemShort?>(null);
        }

        public Task<Statistic?> GetStatisticAsync(string slug, CancellationToken ct = default)
        {
            return Task.FromResult<Statistic?>(null);
        }

        public void Invalidate() { }
        public double? GetReferencePrice(Statistic? stat)
        {
            return stat?.Payload?.StatisticsClosed?.Day90 is { Length: > 0 }
            ? (double?)stat.Payload.StatisticsClosed.Day90[0].Median : null;
        }

        public double? GetMaxReferencePrice(Statistic? stat)
        {
            return GetReferencePrice(stat);
        }

        public void SetStatisticPriority(string slug, Microsoft.Extensions.Caching.Memory.CacheItemPriority priority)
        {
        }
    }

    private static BunitContext CreateCtx(Statistic? stat)
    {
        var ctx = new BunitContext();
        ctx.Services.AddMasaBlazor();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddSingleton<IItemSearchService>(new FakeItemSearch());
        ctx.Services.AddSingleton<IUserOrderService>(new NoopUserOrderService());
        return ctx;
    }

    /// <summary>LoadPricesAsync noop（价格状态由测试直接构造）</summary>
    private sealed class NoopUserOrderService : IUserOrderService
    {
        public Task<UserSearchResult> SearchUserAsync(string name, CancellationToken ct = default)
        {
            return Task.FromResult(new UserSearchResult());
        }

        public Task LoadPricesAsync(UserSearchResult result, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }

    private static IRenderedComponent<UserResultTablePanel> Render(BunitContext ctx, Dictionary<string, Statistic?> prices)
    {
        var result = new UserSearchResult {
            Loading = false,
            Orders = new List<Order> { MakeOrder("o1", "item1", 50) },
            ItemCache = new Dictionary<string, ItemShort?> { ["item1"] = MakeItem("slug1", "item1") },
            Prices = prices,
        };
        return ctx.Render<UserResultTablePanel>(p => p
            .Add(m => m.UserName, "测试用户")
            .Add(m => m.result, result));
    }

    /// <summary>数据行全文（按物品名定位，避开分组行）</summary>
    private static string FirstRowText(IRenderedComponent<UserResultTablePanel> cut)
    {
        return cut.FindAll("tbody tr").First(r => r.TextContent.Contains("slug1")).TextContent;
    }

    [Fact]
    public async Task Ref_price_shows_loading_when_not_loaded_yet()
    {
        await using var ctx = CreateCtx(null);
        var cut = Render(ctx, new Dictionary<string, Statistic?>());

        Assert.Contains("加载中", FirstRowText(cut)); // 价格未加载 → 加载中（不是 -）
    }

    [Fact]
    public async Task Ref_price_shows_value_when_loaded()
    {
        await using var ctx = CreateCtx(null);
        var cut = Render(ctx, new Dictionary<string, Statistic?> { ["slug1"] = MakeStat(100) });

        Assert.Contains("100", FirstRowText(cut)); // 有值 → 数值
        Assert.DoesNotContain("加载中", FirstRowText(cut));
    }

    [Fact]
    public async Task Ref_price_shows_failed_when_loaded_but_failed()
    {
        await using var ctx = CreateCtx(null);
        var cut = Render(ctx, new Dictionary<string, Statistic?> { ["slug1"] = null });

        Assert.Contains("加载失败", FirstRowText(cut)); // 加载完成但失败（null 标记）→ 加载失败
        Assert.DoesNotContain("加载中", FirstRowText(cut));
    }
}
