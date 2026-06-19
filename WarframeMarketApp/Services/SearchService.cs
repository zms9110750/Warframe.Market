using zms9110750.TreeCollection.Trie;
using zms9110750.WarframeMarketApi.Models.Items;

namespace WarframeMarketApp.Services;

/// <summary>
/// 物品搜索服务。Trie 前缀树，包含 slug + 所有语言名。
/// </summary>
public class SearchService
{
	private readonly ItemsCacheService _items;
	private Trie? _trie;
	private List<ItemShort>? _allItems;
	private bool _building;

	public SearchService(ItemsCacheService items) => _items = items;

	/// <summary>确保 Trie 已构建</summary>
	private async Task EnsureBuiltAsync()
	{
		if (_trie != null || _building) return;
		_building = true;
		try
		{
			var all = await _items.GetItemsAsync("zh-hans");
			if (all == null) return;

			_allItems = all.ToList();
			_trie = new Trie(['_', ' ', '·', '-']);
			foreach (var item in _allItems)
			{
				_trie.Add(item.Slug);
				_trie.Add(item.Id);
				foreach (var (_, pake) in item.I18n)
					if (!string.IsNullOrEmpty(pake.Name))
						_trie.Add(pake.Name);
			}
		}
		finally { _building = false; }
	}

	/// <summary>搜索匹配的物品</summary>
	public async Task<List<ItemShort>> SearchAsync(string query)
	{
		await EnsureBuiltAsync();
		if (_trie == null || string.IsNullOrWhiteSpace(query) || _allItems == null)
			return new();

		var matched = _trie.Search(query).ToList();
		if (matched.Count == 0) return new();

		// 构建 I18n 名→Item 的快速查找
		var nameMap = new Dictionary<string, ItemShort>(StringComparer.OrdinalIgnoreCase);
		foreach (var item in _allItems)
		{
			foreach (var (_, pake) in item.I18n)
				if (!string.IsNullOrEmpty(pake.Name))
					nameMap.TryAdd(pake.Name, item);
		}

		// 收集匹配的物品（去重）
		var resultSet = new HashSet<string>();
		var results = new List<ItemShort>();

		foreach (var m in matched)
		{
			// 可能是 slug
			var bySlug = _allItems.FirstOrDefault(i => i.Slug.Equals(m, StringComparison.OrdinalIgnoreCase));
			if (bySlug != null && resultSet.Add(bySlug.Id)) { results.Add(bySlug); continue; }

			// 可能是 id
			var byId = _allItems.FirstOrDefault(i => i.Id.Equals(m, StringComparison.OrdinalIgnoreCase));
			if (byId != null && resultSet.Add(byId.Id)) { results.Add(byId); continue; }

			// 可能是 i18n 名
			if (nameMap.TryGetValue(m, out var byName) && resultSet.Add(byName.Id))
				results.Add(byName);
		}

		return results;
	}
}
