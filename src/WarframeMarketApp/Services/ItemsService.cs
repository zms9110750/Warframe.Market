using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Statistics;
using zms9110750.TreeCollection.Trie;
using WarframeMarketApp.Data;

namespace WarframeMarketApp.Services;

/// <summary>
/// 物品业务逻辑：Trie 搜索 + 价格计算。
/// Trie 数据源为 SQLite 缓存（slug + 所有 i18n Name）。
/// 单例：Trie 在内存中长期驻留，DB 访问通过 IServiceScopeFactory。
/// </summary>
public class ItemsService
{
	private readonly CacheService _cache;
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly IMemoryCache _memCache;

	private Trie? _trie;
	private bool _trieBuilt;
	private readonly object _trieLock = new();

	private const string ItemCachePrefix = "item:";

	public ItemsService(CacheService cache, IServiceScopeFactory scopeFactory, IMemoryCache memCache)
	{
		_cache = cache;
		_scopeFactory = scopeFactory;
		_memCache = memCache;
	}

	// ─── Trie 构建（从 SQLite 加载） ───

	private async Task EnsureTrieAsync()
	{
		if (_trieBuilt) return;
		lock (_trieLock)
		{
			if (_trieBuilt) return;
		}

		using var scope = _scopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<WfmDbContext>();

		var items = await db.Items.ToListAsync();

		var trie = new Trie(['_', ' ', '·']);

		foreach (var item in items)
		{
			trie.Add(item.Slug);
			trie.Add(item.Id);
		}

		var translations = await db.ItemTranslations.ToListAsync();
		foreach (var t in translations)
		{
			if (!string.IsNullOrEmpty(t.Name))
				trie.Add(t.Name);
		}

		lock (_trieLock)
		{
			_trie = trie;
			_trieBuilt = true;
		}
		Log.Information("Trie 构建完成：{Items} 个物品", items.Count);
	}

	/// <summary>从 IMemoryCache 或数据库按 key（slug/id/名字）查 ItemShort</summary>
	private async Task<ItemShort?> GetItemByKeyAsync(string key)
	{
		var cacheKey = ItemCachePrefix + key;
		if (_memCache.TryGetValue(cacheKey, out ItemShort? cached))
			return cached;

		using var scope = _scopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<WfmDbContext>();

		ItemShort? item = null;

		item = await db.Items.FirstOrDefaultAsync(i => i.Slug == key);
		if (item == null)
			item = await db.Items.FirstOrDefaultAsync(i => i.Id == key);
		if (item == null)
		{
			var t = await db.ItemTranslations.FirstOrDefaultAsync(t => t.Name == key);
			if (t != null)
				item = await db.Items.FirstOrDefaultAsync(i => i.Id == t.ItemId);
		}

		if (item == null) return null;

		// 填充 I18n
		var translations = await db.ItemTranslations
			.Where(t => t.ItemId == item.Id)
			.ToListAsync();
		foreach (var tr in translations)
		{
			if (Enum.TryParse<Language>(tr.Language, ignoreCase: true, out var lang))
				item.I18n[lang] = tr;
		}

		_memCache.Set(cacheKey, item, TimeSpan.FromMinutes(30));
		return item;
	}

	// ─── 搜索 ───

	public async Task<List<ItemShort>> SearchAsync(string query)
	{
		if (string.IsNullOrWhiteSpace(query)) return new();
		await EnsureTrieAsync();
		if (_trie == null) return new();
		Log.Information("搜索: {Query}", query);

		var terms = query.Split('/', '\\', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		if (terms.Length == 0) return new();

		var resultSet = new HashSet<string>();
		var results = new List<ItemShort>();

		foreach (var term in terms)
		{
			if (term.All(c => "_ ·".Contains(c)))
				continue;

			var matched = _trie.Search(term);
			foreach (var m in matched)
			{
				if (resultSet.Contains(m)) continue;
				var item = await GetItemByKeyAsync(m);
				if (item != null && resultSet.Add(item.Id))
					results.Add(item);
			}
		}

		Log.Information("搜索 {Query}: {Count} 个结果", query, results.Count);
		return results;
	}

	// ─── 统计数据（委托给 CacheService，它有 IMemoryCache） ───

	public async Task<Statistic?> GetStatisticAsync(string itemId, CancellationToken ct = default)
	{
		try
		{
			return await _cache.GetStatisticsAsync(itemId, ct);
		}
		catch { return null; }
	}

	public Statistic? GetStatisticFromCache(string itemId)
	{
		_memCache.TryGetValue("stat:" + itemId, out Statistic? stat);
		return stat;
	}

	// ─── 参考价计算 ───

	public static IReadOnlyList<int> SyntheticConsumption { get; } = [1, 3, 6, 10, 15, 21];
	private static readonly double[] DefaultWeight = [40, 25, 15, 5, 5, 5, 5];

	public double? GetReferencePrice(Statistic? stat)
	{
		if (stat?.Payload?.StatisticsClosed?.Day90 == null) return null;
		return CalcWeightedMedian(stat.Payload.StatisticsClosed.Day90,
			e => e.ModRank is null or 0);
	}

	public double? GetMaxReferencePrice(Statistic? stat)
	{
		if (stat?.Payload?.StatisticsClosed?.Day90 == null) return null;
		return CalcWeightedMedian(stat.Payload.StatisticsClosed.Day90,
			e => e.ModRank is > 0 &&
				 (e.Subtype is null or "crafted" or "radiant" or "magnificent" or "large"));
	}

	public double? GetReferencePriceFiltered(Statistic? stat, Func<Entry, bool> filter)
	{
		if (stat?.Payload?.StatisticsClosed?.Day90 == null) return null;
		return CalcWeightedMedian(stat.Payload.StatisticsClosed.Day90, filter);
	}

	public double? GetMaterialBasedReferencePrice(Statistic? stat)
	{
		var max = GetMaxReferencePrice(stat);
		if (max == null) return null;

		var firstRanked = stat?.Payload?.StatisticsClosed?.Day90
			?.FirstOrDefault(e => e.ModRank > 0);
		var rank = firstRanked?.ModRank;
		if (rank is > 0 and <= 5)
			return max / SyntheticConsumption[rank.Value];
		return max;
	}

	static double? CalcWeightedMedian(Entry[] day90, Func<Entry, bool> filter)
	{
		var entries = day90
			.Where(filter)
			.OrderByDescending(e => e.Datetime)
			.Take(7)
			.ToArray();

		if (entries.Length == 0) return null;

		double totalWeight = 0, weightedSum = 0;
		for (int i = 0; i < entries.Length; i++)
		{
			var w = DefaultWeight[i] * entries[i].Volume;
			totalWeight += w;
			weightedSum += w * entries[i].Median;
		}
		return totalWeight > 0 ? weightedSum / totalWeight : null;
	}
}
