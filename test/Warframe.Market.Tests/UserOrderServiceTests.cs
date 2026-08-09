using Xunit;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Orders;
using zms9110750.WarframeMarketApi.Models.Statistics;
using zms9110750.WarframeMarketApi.Models.Users;
using zms9110750.WarframeMarketApi.Services;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>
/// UserOrderService.LoadPricesAsync 测试：价格分批填充、批内去重、状态复位、空订单。
/// （SearchUserAsync 拆分后价格加载独立公开，此为拆出来的可单测部分）
/// </summary>
public class UserOrderServiceTests
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

    /// <summary>可计数的假 IItemSearchService：统计每次 GetStatisticAsync 调用</summary>
    private sealed class CountingItemSearch : IItemSearchService
    {
        public Statistic? Stat { get; set; }

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
            Interlocked.Increment(ref _calls);
            return Task.FromResult(Stat);
        }
        private int _calls;
        public int Calls => _calls;

        public void Invalidate()
        {
        }

        public double? GetReferencePrice(Statistic? stat)
        {
            return stat?.GetReferencePrice();
        }

        public double? GetMaxReferencePrice(Statistic? stat)
        {
            return stat?.GetMaxReferencePrice();
        }

        public void SetStatisticPriority(string slug, Microsoft.Extensions.Caching.Memory.CacheItemPriority priority)
        {
        }
    }

    private static UserOrderService CreateService(CountingItemSearch items)
    {
        var client = new WarframeMarketClient(new HttpClient(new FakeHttpMessageHandler()) {
            BaseAddress = new Uri("https://api.warframe.market")
        });
        return new UserOrderService(client, items);
    }

    [Fact]
    public async Task LoadPricesAsync_fills_prices_for_all_orders()
    {
        var items = new CountingItemSearch { Stat = MakeStat(100) };
        var svc = CreateService(items);

        var result = new UserSearchResult {
            Loading = false,
            Orders = new List<Order> {
                MakeOrder("o1", "item1", 50),
                MakeOrder("o2", "item2", 80),
                MakeOrder("o3", "item3", 120),
            },
            ItemCache = new Dictionary<string, ItemShort?> {
                ["item1"] = MakeItem("slug1", "item1"),
                ["item2"] = MakeItem("slug2", "item2"),
                ["item3"] = MakeItem("slug3", "item3"),
            },
        };

        await svc.LoadPricesAsync(result);

        Assert.Equal(3, result.Prices.Count);
        Assert.Contains("slug1", result.Prices.Keys);
        Assert.Contains("slug2", result.Prices.Keys);
        Assert.Contains("slug3", result.Prices.Keys);
        Assert.Equal(3, items.Calls);
        Assert.False(result.LoadingPrices); // 完成后复位
    }

    [Fact]
    public async Task LoadPricesAsync_deduplicates_same_slug_within_batch()
    {
        var items = new CountingItemSearch { Stat = MakeStat(100) };
        var svc = CreateService(items);

        // 同一物品多个订单（同 slug）：只请求一次价格
        var result = new UserSearchResult {
            Orders = new List<Order> {
                MakeOrder("o1", "item1", 50),
                MakeOrder("o2", "item1", 60),
                MakeOrder("o3", "item1", 70),
            },
            ItemCache = new Dictionary<string, ItemShort?> { ["item1"] = MakeItem("slug1", "item1") },
        };

        await svc.LoadPricesAsync(result);

        Assert.Single(result.Prices);
        Assert.Equal(1, items.Calls); // 去重：只请求一次
        Assert.False(result.LoadingPrices);
    }

    [Fact]
    public async Task LoadPricesAsync_empty_orders_is_noop()
    {
        var items = new CountingItemSearch { Stat = MakeStat(100) };
        var svc = CreateService(items);

        var result = new UserSearchResult { Orders = new List<Order>() };

        await svc.LoadPricesAsync(result);

        Assert.Empty(result.Prices);
        Assert.Equal(0, items.Calls);
        Assert.False(result.LoadingPrices);
    }

    [Fact]
    public async Task LoadPricesAsync_missing_item_cache_skips()
    {
        var items = new CountingItemSearch { Stat = MakeStat(100) };
        var svc = CreateService(items);

        // 订单的 itemId 不在 ItemCache（找不到物品）→ 跳过价格请求
        var result = new UserSearchResult {
            Orders = new List<Order> { MakeOrder("o1", "unknown-item", 50) },
            ItemCache = new Dictionary<string, ItemShort?>(),
        };

        await svc.LoadPricesAsync(result);

        Assert.Empty(result.Prices);
        Assert.Equal(0, items.Calls);
        Assert.False(result.LoadingPrices);
    }

    [Fact]
    public async Task LoadPricesAsync_keeps_existing_prices()
    {
        var items = new CountingItemSearch { Stat = MakeStat(100) };
        var svc = CreateService(items);

        // 已加载过的价格（Prices 已有）不重复请求
        var result = new UserSearchResult {
            Orders = new List<Order> { MakeOrder("o1", "item1", 50) },
            ItemCache = new Dictionary<string, ItemShort?> { ["item1"] = MakeItem("slug1", "item1") },
            Prices = new Dictionary<string, Statistic?> { ["slug1"] = MakeStat(200) },
        };

        await svc.LoadPricesAsync(result);

        Assert.Single(result.Prices);
        Assert.Equal(0, items.Calls); // 已存在 → 不请求
        Assert.False(result.LoadingPrices);
    }
}
