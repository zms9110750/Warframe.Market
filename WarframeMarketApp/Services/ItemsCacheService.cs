using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Statistics;

namespace WarframeMarketApp.Services;

/// <summary>
/// 物品缓存服务。整合版本对比 + 物品列表缓存 + 翻译缓存。
/// </summary>
public class ItemsCacheService
{
	private readonly WarframeMarketClient _wfm;
	private readonly LocalCacheService _cache;

	public ItemsCacheService(WarframeMarketClient wfm, LocalCacheService cache)
	{
		_wfm = wfm;
		_cache = cache;
	}

	// ─── 缓存键 ───

	private static string ItemsKey(string lang) => $"items:{lang}";
	private static string ItemKey(string slug) => $"item:{slug}";
	private static string ItemSetKey(string slug) => $"itemset:{slug}";
	private static string StatsKey(string slug) => $"stats:{slug}";

	// ─── 物品列表 ───

	public async Task<ItemShort[]?> GetItemsAsync(string lang, CancellationToken ct = default)
	{
		var key = ItemsKey(lang);
		var items = await _cache.GetAsync<ItemShort[]>(key);
		if (items != null) return items;

		var resp = await _wfm.GetItemsAsync(ct);
		if (resp?.Content?.Data == null) return null;

		// 写回缓存
		await _cache.SetAsync(key, resp.Content.Data);
		return resp.Content.Data;
	}

	public async Task SetItemsAsync(string lang, ItemShort[] items)
	{
		await _cache.SetAsync(ItemsKey(lang), items);
	}

	// ─── 单物品 ───

	public async Task<Item?> GetItemAsync(string slug, CancellationToken ct = default)
	{
		var key = ItemKey(slug);
		var item = await _cache.GetAsync<Item>(key);
		if (item != null) return item;

		var resp = await _wfm.GetItemAsync(slug, ct);
		if (resp?.Content?.Data == null) return null;

		await _cache.SetAsync(key, resp.Content.Data);
		return resp.Content.Data;
	}

	// ─── 套装 ───

	public async Task<ItemSet?> GetItemSetAsync(string slug, CancellationToken ct = default)
	{
		var key = ItemSetKey(slug);
		var set = await _cache.GetAsync<ItemSet>(key);
		if (set != null) return set;

		var resp = await _wfm.GetItemSetAsync(slug, ct);
		if (resp?.Content?.Data == null) return null;

		await _cache.SetAsync(key, resp.Content.Data);
		return resp.Content.Data;
	}

	// ─── 统计数据 ───

	public async Task<Response<Statistic>?> GetStatisticsAsync(string slug, CancellationToken ct = default)
	{
		var key = StatsKey(slug);
		var stats = await _cache.GetAsync<Response<Statistic>>(key);
		if (stats != null) return stats;

		var data = await _wfm.GetStatisticsAsync(slug, ct);
		await _cache.SetAsync(key, data);
		return data;
	}

	/// <summary>
	/// 搜索物品。按 slug 或翻译名匹配
	/// </summary>
	public async Task<List<ItemShort>> SearchAsync(string query, CancellationToken ct = default)
	{
		// 确保缓存数据就绪
		var items = await GetItemsAsync("zh-hans", ct);
		if (items == null) return new();

		var q = query.ToLowerInvariant();
		return items.Where(i =>
			i.Slug.Contains(q) ||
			(i.I18n.TryGetValue(Language.ZhHans, out var zh) &&
			 zh.Name.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
			(i.I18n.TryGetValue(Language.En, out var en) &&
			 en.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
		).ToList();
	}
}
