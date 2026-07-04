
using System.Collections.Concurrent;
using WarframeMarketQuery;
using WarframeMarketQuery.Model;
using WarframeMarketQuery.Model.Items;
using WarframeMarketQuery.Model.Orders;
using WarframeMarketQuery.Model.Statistics;
using ZiggyCreatures.Caching.Fusion;
using zms9110750.TreeCollection.Trie;
using Version = WarframeMarketQuery.Model.Versions.Version;
namespace WarframeMarketQuery.API;

/// <summary>
/// 使用 Refit 接口 (V2/V1) 访问市场 API 的实现，
/// 不再直接使用 HttpClient，缓存中存储纯 T（而非 Response&lt;T&gt;）
/// </summary>
public class WarframeMarketApi(
    IWarframeMarketApi apiV2,
    IWarframeMarketApiV1 apiV1,
    IFusionCache fusion,
    Trie? trie = null)
{
    IWarframeMarketApi ApiV2 { get; } = apiV2;
    IWarframeMarketApiV1 ApiV1 { get; } = apiV1;
    IFusionCache Fusion { get; } = fusion;
    Trie? Trie { get; } = trie;

    FusionCacheEntryOptions SkippingDistributed { get; } = new FusionCacheEntryOptions()
           .SetSkipDistributedCache(true, null)
           .SetDistributedCacheDurationZero()
           .SetFailSafe(true, TimeSpan.FromSeconds(5));

    /// <summary>
    /// 从物品索引获取物品的slug（从缓存读取）
    /// </summary>
    public ValueTask<string> GetSlugByIndexAsync(
        string index,
        CancellationToken cancellation = default)
    {
        return Fusion.GetOrDefaultAsync<string>("index:" + index, token: cancellation)!;
    }

    /// <summary>
    /// 根据物品索引获取物品短信息（从缓存读取）
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public async ValueTask<ItemShort> GetItemByIndexAsync(
        string index,
        CancellationToken cancellation = default)
    {
        var slug = await GetSlugByIndexAsync(index, cancellation);
        return await Fusion.GetOrDefaultAsync<ItemShort>("item:" + slug, token: cancellation)
               ?? throw new ArgumentException("Item not find");
    }

    /// <summary>
    /// 获取 API 版本的 Data 部分（Version 实例），使用跳过分布式缓存的策略
    /// 缓存键："versions"
    /// 返回值为 Version (models 中的类型)
    /// </summary>
    public ValueTask<Version> GetVersionAsync(
        CancellationToken cancellation = default)
    {
        return Fusion.GetOrSetAsync(
            "versions",
            async ct => (await ApiV2.GetVersionsAsync(ct)).Content!.Data,
            SkippingDistributed,
            token: cancellation)!;
    }

    /// <summary>
    /// 强制更新物品索引并覆盖缓存。返回 Task 并在全部写入完成后才写入版本号，
    /// 避免版本号先写入导致数据残缺却不触发自动更新的情况。
    /// </summary>
    public async Task GetAndSetIndexByItemAsync(
        CancellationToken cancellation = default)
    {
        var resp = await ApiV2.GetItemsAsync(cancellation);
        var items = resp.Content!;
        var trie = new ConcurrentBag<string>();
        var version = GetVersionAsync(cancellation);

        await Parallel.ForEachAsync(
            items.Data,
            cancellation,
            async (item, ct) =>
            {
                await Fusion.SetAsync(
                    "item:" + item.Slug,
                    item,
                    tags: [nameof(Item).ToLower()],
                    token: ct);

                await Fusion.SetAsync(
                    "index:" + item.Slug,
                    item.Slug,
                    tags: [nameof(Item).ToLower(), "index", nameof(item.Slug).ToLower()],
                    token: ct);

                await Fusion.SetAsync(
                    "index:" + item.Id,
                    item.Slug,
                    tags: [nameof(Item).ToLower(), "index", nameof(item.Id).ToLower()],
                    token: ct);

                trie.Add(item.Slug);
                trie.Add(item.Id);

                foreach (var i18n in item.I18n)
                {
                    await Fusion.SetAsync(
                        "index:" + i18n.Value.Name,
                        item.Slug,
                        tags: [nameof(Item).ToLower(), "index", nameof(item.I18n).ToLower(), i18n.Key.ToString().ToLower()],
                        token: ct);

                    trie.Add(i18n.Value.Name);
                }
            });

        // 在所有 item/index 写入完成后再写入 trie 与版本号，保证不会出现版本先写入导致数据残缺的问题
        await Fusion.SetAsync(
            nameof(Trie).ToLower(),
            trie,
            tags: [nameof(Item).ToLower(), "trie"],
            token: cancellation);

        await Fusion.SetAsync(
            nameof(Version).ToLower(),
            await version,
            tags: [nameof(Item).ToLower(), nameof(Version).ToLower()],
            token: cancellation);

        if (Trie != null)
        {
            foreach (var item in trie)
            {
                Trie.Add(item);
            }
        }
    }

    /// <summary>
    /// 获取指定物品可见订单（缓存仅存 Order[]，返回 Order[]）
    /// 缓存键："orders:item:{slug}"
    /// </summary>
    public ValueTask<Order[]> GetOrdersItemAsync(
        string slug,
        CancellationToken cancellation = default)
    {
        return Fusion.GetOrSetAsync(
            $"orders:item:{slug}",
            async ct => (await ApiV2.GetOrdersItemAsync(slug, ct)).Content!.Data,
            SkippingDistributed,
            token: cancellation)!;
    }

    /// <summary>
    /// 获取指定物品的在线用户中买单/卖单前5个，可带查询参数（统一方法）
    /// 使用 OrderTopQueryParameter.ToString() 作为 key 后缀（稳定）
    /// 缓存键：
    ///   无 query -> "orders:item:{slug}:top"
    ///   有 query -> "orders:item:{slug}:top:{query.ToString()}"
    /// 返回 OrderTop
    /// </summary>
    public ValueTask<OrderTop> GetOrdersItemTopAsync(
        string slug,
        OrderTopQueryParameter? query = null,
        CancellationToken cancellation = default)
    {
        var key = $"orders:item:{slug}:top:{query}";

        return Fusion.GetOrSetAsync(
            key,
            async ct => (await ApiV2.GetOrdersItemTopAsync(slug, query, ct)).Content!.Data,
            SkippingDistributed,
            token: cancellation)!;
    }

    /// <summary>
    /// 获取用户的公开订单（缓存仅存 Order[]，返回 Order[]）
    /// 缓存键："orders:user:{slug}"
    /// </summary>
    public ValueTask<Order[]?> GetOrdersFromUserAsync(
        string slug,
        CancellationToken cancellation = default)
    {
        return Fusion.GetOrSetAsync(
            $"orders:user:{slug}",
            async ct => (await ApiV2.GetOrdersFromUserAsync(slug, ct)).Content?.Data,
            SkippingDistributed,
            token: cancellation)!;
    }

    /// <summary>
    /// 获取物品订单统计数据（使用 IWarframeMarketApiV1）
    /// 缓存直接存储 Statistic，并返回 Statistic。
    /// 缓存键："items:{slug}:statistics"
    /// </summary>
    public ValueTask<Statistic> GetStatisticAsync(
        string slug,
        CancellationToken cancellation = default)
    {
        return Fusion.GetOrSetAsync(
            $"items:{slug}:statistics",
            async ct => (await ApiV1.GetStatisticAsync(slug, ct)).Content!,
            options => options
                .SetDuration(CalcSoftExpiration())
                .SetFailSafe(true, TimeSpan.FromDays(2)),
            token: cancellation)!;
    }

    /// <summary>
    /// 根据索引获取物品的统计数据
    /// </summary>
    public async ValueTask<Statistic> GetStatisticByIndexAsync(
        string index,
        CancellationToken cancellation = default)
    {
        var slug = await GetSlugByIndexAsync(index, cancellation);
        return await GetStatisticAsync(slug, cancellation);
    }

    static TimeSpan CalcSoftExpiration()
    {
        var utcNow = DateTime.UtcNow.TimeOfDay;
        return TimeSpan.FromHours((utcNow.Hours + 2) / 2 * 2) - utcNow;
    }
}