using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Statistics;
using zms9110750.TreeCollection.Trie;

namespace WarframeMarketApp.Services;

/// <summary>
/// 物品业务逻辑。包装 API 调用，提供搜索、价格计算。
/// </summary>
public class ItemsService
{
	private readonly WarframeMarketClient _wfm;
	private List<ItemShort>? _cachedItems;
	private TrieWrapper? _trie;

	public ItemsService(WarframeMarketClient wfm) => _wfm = wfm;

	// ─── 物品缓存 ───

	public async Task<List<ItemShort>> GetItemsAsync()
	{
		if (_cachedItems != null) return _cachedItems;
		var resp = await _wfm.GetItemsAsync();
		_cachedItems = resp?.Content?.Data?.ToList() ?? new();
		return _cachedItems;
	}

	// ─── Trie 搜索 ───

	public async Task<List<ItemShort>> SearchAsync(string query)
	{
		var items = await GetItemsAsync();
		if (_trie == null)
		{
			_trie = new TrieWrapper();
			foreach (var item in items)
			{
				_trie.Add(item.Slug);
				_trie.Add(item.Id);
				foreach (var (_, p) in item.I18n)
					if (!string.IsNullOrEmpty(p.Name))
						_trie.Add(p.Name);
			}
		}

		var matched = _trie.Search(query);
		if (matched.Count == 0) return new();

		var slugMap = items.ToDictionary(i => i.Slug, StringComparer.OrdinalIgnoreCase);
		var idMap = items.ToDictionary(i => i.Id, StringComparer.OrdinalIgnoreCase);
		var nameMap = new Dictionary<string, ItemShort>(StringComparer.OrdinalIgnoreCase);
		foreach (var item in items)
			foreach (var (_, p) in item.I18n)
				if (!string.IsNullOrEmpty(p.Name))
					nameMap.TryAdd(p.Name, item);

		var seen = new HashSet<string>();
		var result = new List<ItemShort>();
		foreach (var m in matched)
		{
			ItemShort? found = null;
			if (slugMap.TryGetValue(m, out var bySlug) && seen.Add(bySlug.Id)) found = bySlug;
			else if (idMap.TryGetValue(m, out var byId) && seen.Add(byId.Id)) found = byId;
			else if (nameMap.TryGetValue(m, out var byName) && seen.Add(byName.Id)) found = byName;
			if (found != null) result.Add(found);
		}
		return result;
	}

	// ─── 统计数据 + 价格计算 ───

	private Dictionary<string, Statistic?> _statsCache = new();

	public async Task<Statistic?> GetStatisticAsync(string slug)
	{
		if (_statsCache.TryGetValue(slug, out var cached)) return cached;
		try
		{
			var resp = await _wfm.GetStatisticsAsync(slug);
			var stat = resp?.Data;
			_statsCache[slug] = stat;
			return stat;
		}
		catch { return null; }
	}

	/// <summary>参考价：已结算90天，无等级，加权中位数</summary>
	public double? GetReferencePrice(Statistic? stat)
	{
		if (stat?.Payload?.StatisticsClosed?.Day90 == null) return null;
		var entries = stat.Payload.StatisticsClosed.Day90
			.Where(e => e.ModRank is null or 0)
			.OrderByDescending(e => e.Datetime)
			.Take(7).ToArray();
		if (entries.Length == 0) return null;
		double[] ws = [40, 25, 15, 5, 5, 5, 5];
		double tw = 0, ws_ = 0;
		for (int i = 0; i < entries.Length; i++)
		{
			var w = ws[i] * entries[i].Volume;
			tw += w;
			ws_ += w * entries[i].Median;
		}
		return ws_ / tw;
	}

	/// <summary>满级价：已结算90天，最⾼等级的中位数</summary>
	public double? GetMaxPrice(Statistic? stat)
	{
		if (stat?.Payload?.StatisticsClosed?.Day90 == null) return null;
		var max = stat.Payload.StatisticsClosed.Day90
			.Where(e => e.ModRank > 0)
			.OrderByDescending(e => e.ModRank)
			.FirstOrDefault();
		return max?.Median;
	}
}

/// <summary>
/// 简单的 Trie 包装，避免依赖 zms9110750.TreeCollection
/// </summary>
internal class TrieWrapper
{
	private readonly HashSet<string> _words = new(StringComparer.OrdinalIgnoreCase);

	public void Add(string word)
	{
		_words.Add(word);
	}

	public List<string> Search(string prefix)
	{
		if (string.IsNullOrEmpty(prefix)) return new();
		return _words.Where(w => w.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).Take(100).ToList();
	}
}
