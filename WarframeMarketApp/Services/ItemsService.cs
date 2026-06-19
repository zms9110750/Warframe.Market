using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using zms9110750.WarframeMarketApi;
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
	private readonly WarframeMarketClient _wfm;
	private readonly CacheService _cache;
	private readonly IServiceScopeFactory _scopeFactory;

	private Trie? _trie;
	private Dictionary<string, ItemShort>? _slugMap;
	private bool _trieBuilt;
	private readonly object _trieLock = new();

	public ItemsService(WarframeMarketClient wfm, CacheService cache, IServiceScopeFactory scopeFactory)
	{
		_wfm = wfm;
		_cache = cache;
		_scopeFactory = scopeFactory;
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
		_slugMap = items.ToDictionary(i => i.Slug, StringComparer.OrdinalIgnoreCase);

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
	}

	// ─── 搜索 ───

	public async Task<List<ItemShort>> SearchAsync(string query)
	{
		if (string.IsNullOrWhiteSpace(query)) return new();
		await EnsureTrieAsync();
		if (_trie == null || _slugMap == null) return new();

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
				if (_slugMap.TryGetValue(m, out var item) && resultSet.Add(item.Id))
					results.Add(item);
			}
		}

		return results;
	}

	// ─── 统计数据 ───

	private readonly Dictionary<string, Statistic?> _statsCache = new();

	public async Task<Statistic?> GetStatisticAsync(string itemId, CancellationToken ct = default)
	{
		if (_statsCache.TryGetValue(itemId, out var cached))
			return cached;
		try
		{
			var stat = await _cache.GetStatisticsAsync(itemId, ct);
			_statsCache[itemId] = stat;
			return stat;
		}
		catch { return null; }
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
				 (e.Subtype is "crafted" or "radiant" or "magnificent" or "large"));
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
