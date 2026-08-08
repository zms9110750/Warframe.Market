using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Arcane;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Statistics;
using zms9110750.WarframeMarketApi.Services;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>
/// 领域服务接口单元测试：UI 的每个触发调用的接口方法，在此用本地备份假数据验证（不走 UI/Masa）。
/// </summary>
public class ServiceTests
{
    private static (FakeHttpMessageHandler Handler, WarframeMarketClient Client) CreatePair()
    {
        var handler = new FakeHttpMessageHandler();
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.warframe.market") };
        return (handler, new WarframeMarketClient(http));
    }

    // ─── IItemSearchService ───

    [Fact]
    public async Task ItemSearch_SearchAsync_returns_results()
    {
        var (handler, client) = CreatePair();
        handler.Map("/v2/items", Data.File("items", "items.json"));
        var svc = new ItemSearchService(client);

        var results = await svc.SearchAsync("wisp");

        Assert.NotEmpty(results);
    }

    [Fact]
    public async Task ItemSearch_FindByKey_normalized_name_matches()
    {
        var (handler, client) = CreatePair();
        handler.Map("/v2/items", Data.File("items", "items.json"));
        var svc = new ItemSearchService(client);

        // 归一化：空格差异的写法应命中（"Secura  Dual Cestra"≡"Secura Dual Cestra"）
        var item = await svc.FindByKeyAsync("Secura  Dual Cestra");

        Assert.NotNull(item);
        Assert.Equal("secura_dual_cestra", item!.Slug);
    }

    [Fact]
    public async Task ItemSearch_GetStatistic_and_reference_price()
    {
        var (handler, client) = CreatePair();
        handler.Map("/v2/items", Data.File("items", "items.json"));
        var svc = new ItemSearchService(client);
        var slug = "secura_dual_cestra";
        handler.Map($"/v1/items/{slug}/statistics", Data.File("statistics", "secura_dual_cestra.json"));

        var stat = await svc.GetStatisticAsync(slug);

        Assert.NotNull(stat);
        Assert.NotNull(svc.GetReferencePrice(stat));
        Assert.True(svc.GetReferencePrice(stat) > 0);
    }

    // ─── IUserOrderService ───

    [Fact]
    public async Task UserOrder_SearchUser_returns_user_orders_and_items()
    {
        var (handler, client) = CreatePair();
        var slug = "zyzo_o";
        handler.Map("/v2/items", Data.File("items", "items.json"));
        handler.Map($"/v2/user/{slug}", Data.File("users", "user.json"));
        handler.Map($"/v2/orders/user/{slug}", Data.File("orders", "orders-user.json"));
        var itemsSvc = new ItemSearchService(client);
        var svc = new UserOrderService(client, itemsSvc);

        var result = await svc.SearchUserAsync(slug);

        Assert.False(result.NotFound);
        Assert.False(result.Error != null);
        Assert.NotNull(result.User);
        Assert.Equal(slug, result.User!.Slug);
        Assert.NotNull(result.Orders);
        Assert.NotEmpty(result.Orders);
        // 物品已从本地索引补齐（不额外走 API）
        Assert.NotEmpty(result.ItemCache);
    }

    [Fact]
    public async Task UserOrder_SearchUser_not_found()
    {
        var (handler, client) = CreatePair();
        handler.Map("/v2/items", Data.File("items", "items.json"));
        var itemsSvc = new ItemSearchService(client);
        var svc = new UserOrderService(client, itemsSvc);

        var result = await svc.SearchUserAsync("no_such_user_xyz");

        Assert.True(result.NotFound, $"Error={result.Error} NotFound={result.NotFound}");
    }

    // ─── IArcanePackService（假 IItemSearchService 测计算） ───

    private sealed class FakeItemSearch : IItemSearchService
    {
        public List<ItemShort> Results { get; init; } = new();
        public Statistic? Stat { get; init; }
        public double? MaxPrice { get; init; }

        public Task<List<ItemShort>> SearchAsync(string query, CancellationToken ct = default)
        {
            return Task.FromResult(Results);
        }

        public Task<ItemShort?> FindByKeyAsync(string key)
        {
            return Task.FromResult(Results.FirstOrDefault());
        }

        public Task<Statistic?> GetStatisticAsync(string slug, CancellationToken ct = default)
        {
            return Task.FromResult(Stat);
        }

        public double? GetReferencePrice(Statistic? stat)
        {
            return stat?.GetReferencePrice();
        }

        public double? GetMaxReferencePrice(Statistic? stat)
        {
            return MaxPrice ?? stat?.GetMaxReferencePrice();
        }

        public void Invalidate() { }
    }

    [Fact]
    public async Task ArcanePack_reference_price_is_positive()
    {
        var fake = new FakeItemSearch {
            Results = new List<ItemShort>
            {
                new("id1", "slug1", "GameRef", new HashSet<string>(), 0, null, null, null, null, null, null, null,
                    new Dictionary<Language, LanguagePake>())
            },
            Stat = CreateFakeStat(100),
            MaxPrice = 200,
        };
        var svc = new ArcanePackService(fake);
        var pack = new ArcanePackConfig {
            Name = "测试包",
            Items =
            [
                new ArcaneQualityGroup { Subtypes = "Common", Quality = 1.0, Items = ["物品一"] }
            ],
        };

        var value = await svc.GetReferencePriceAsync(pack, purchase: 0);

        Assert.True(value > 0);
        Assert.Equal(200 * 1.0 * ArcanePackService.PackGainRate, value, 0.001);
    }

    [Fact]
    public async Task ArcanePack_unknown_item_contributes_zero()
    {
        var svc = new ArcanePackService(new FakeItemSearch()); // 找不到物品
        var pack = new ArcanePackConfig {
            Name = "空包",
            Items = [new ArcaneQualityGroup { Subtypes = "Common", Quality = 1.0, Items = ["不存在的物品"] }],
        };

        var value = await svc.GetReferencePriceAsync(pack);

        Assert.Equal(0, value);
    }

    private static Statistic CreateFakeStat(double median)
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
}
