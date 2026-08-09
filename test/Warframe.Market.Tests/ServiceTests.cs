using Microsoft.Extensions.Caching.Memory;
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

    [Fact]
    public async Task ItemSearch_GetStatistic_cached_until_priority_demoted()
    {
        // 统计缓存：MS IMemoryCache（独立实例，可逐出）。串行同 slug 命中缓存（HTTP 1 次）。
        var (handler, client) = CreatePair();
        var slug = "secura_dual_cestra";
        handler.Map($"/v1/items/{slug}/statistics", Data.File("statistics", "secura_dual_cestra.json"));
        var cache = new MemoryCache(new MemoryCacheOptions());
        var svc = new ItemSearchService(client, statCache: cache);

        var first = await svc.GetStatisticAsync(slug);
        var second = await svc.GetStatisticAsync(slug);

        Assert.NotNull(first);
        Assert.NotNull(second);
        // 缓存命中：第二次不再发 HTTP
        Assert.Equal(1, handler.RequestUris.Count(u => u.PathAndQuery.Contains("/statistics")));

        // 组件关闭（路由离开）→ 降级为 Normal：条目仍在缓存（第三次仍命中）
        svc.SetStatisticPriority(slug, CacheItemPriority.Normal);
        var third = await svc.GetStatisticAsync(slug);
        Assert.NotNull(third);
        Assert.Equal(1, handler.RequestUris.Count(u => u.PathAndQuery.Contains("/statistics")));
    }

    [Fact]
    public async Task ItemSearch_GetStatistic_concurrent_returns_consistent_results()
    {
        // 并发同 slug：MS MemoryCache 无工厂去重（去重由 HTTP 缓存层承担），
        // 但结果一致、不崩（FusionCache 曾在此并发路径 NRE → 赋能包全失败）
        var (handler, client) = CreatePair();
        var slug = "secura_dual_cestra";
        handler.Map($"/v1/items/{slug}/statistics", Data.File("statistics", "secura_dual_cestra.json"));
        var svc = new ItemSearchService(client, statCache: new MemoryCache(new MemoryCacheOptions()));

        var tasks = Enumerable.Range(0, 10).Select(_ => svc.GetStatisticAsync(slug)).ToArray();
        var stats = await Task.WhenAll(tasks);

        Assert.All(stats, s => Assert.NotNull(s));
    }

    [Fact]
    public async Task ItemSearch_GetStatistic_failure_returns_null_without_crash()
    {
        // 统计 404（未 Map）：返回 null 不崩且被缓存（FusionCache 曾在此路径 NRE → 赋能包全失败）
        var (handler, client) = CreatePair();
        var slug = "no_such_slug";
        var svc = new ItemSearchService(client, statCache: new MemoryCache(new MemoryCacheOptions()));

        var first = await svc.GetStatisticAsync(slug);
        var second = await svc.GetStatisticAsync(slug);

        Assert.Null(first);
        Assert.Null(second);
        // 失败（null）也被缓存 → 只发 1 次请求（避免失败风暴）
        Assert.Equal(1, handler.RequestUris.Count(u => u.PathAndQuery.Contains("/statistics")));
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

        public void SetStatisticPriority(string slug, Microsoft.Extensions.Caching.Memory.CacheItemPriority priority)
        {
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

    [Fact]
    public async Task ArcanePack_purchase_caps_expected_value_by_daily_volume()
    {
        // purchase>0 时按 日成交量/购买量 封顶有效量（防"开箱量超过市场流通量"期望虚高）
        var fake = new FakeItemSearch {
            Results = new List<ItemShort>
            {
                new("id1", "slug1", "GameRef", new HashSet<string>(), 0, null, null, null, null, null, null, null,
                    new Dictionary<Language, LanguagePake>())
            },
            Stat = CreateFakeStat(100), // Day90 7 条，Volume=10/条 → GetDailyVolume = 70/90 ≈ 0.7778
            MaxPrice = 200,
        };
        var svc = new ArcanePackService(fake);
        var pack = new ArcanePackConfig {
            Name = "测试包",
            Items = [new ArcaneQualityGroup { Subtypes = "Common", Quality = 1.0, Items = ["物品一"] }],
        };

        var uncapped = await svc.GetReferencePriceAsync(pack, purchase: 0); // 150 = 200×1.0×PackGainRate
        var capped = await svc.GetReferencePriceAsync(pack, purchase: 10); // 封顶 ≈ 200×(70/90/10) ≈ 15.6

        Assert.True(uncapped > 0);
        Assert.True(capped > 0);
        Assert.True(capped < uncapped); // 封顶后显著小于未封顶
        Assert.Equal(200 * (70.0 / 90 / 10), capped, 0.001);
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
