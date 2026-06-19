using System.Text.Json;
using System.Text.Json.Serialization;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.TreeCollection.Trie;

var wfm = new WarframeMarketClient();

Console.Error.WriteLine("=== 获取物品列表 ===");
var resp = await wfm.GetItemsAsync();
var items = resp?.Content?.Data ?? throw new Exception("API 失败");
Console.Error.WriteLine($"共 {items.Length} 个物品\n");

// 构建 Trie
var trie = new Trie(['_', ' ', '·', '-']);
foreach (var item in items)
{
	trie.Add(item.Slug);
	trie.Add(item.Id);
	foreach (var (_, pake) in item.I18n)
		if (!string.IsNullOrEmpty(pake.Name))
			trie.Add(pake.Name);
}
Console.Error.WriteLine($"Trie 构建完成\n");

// 测试三个查询
string[] queries = ["盲怒", "wisp", "镀层"];
foreach (var q in queries)
{
	Console.Error.WriteLine($"\n=== 查询: \"{q}\" ===");
	var matched = trie.Search(q).ToList();
	Console.Error.WriteLine($"  匹配 {matched.Count} 个");

	foreach (var m in matched.Take(20))
	{
		// 找到这个 slug/id/name 对应的物品
		var item = items.FirstOrDefault(i =>
			i.Slug.Equals(m, StringComparison.OrdinalIgnoreCase) ||
			i.Id.Equals(m, StringComparison.OrdinalIgnoreCase));

		if (item != null)
		{
			var zh = item.I18n.TryGetValue(Language.ZhHans, out var z) ? z.Name : "";
			var en = item.I18n.TryGetValue(Language.En, out var e) ? e.Name : "";
			Console.Error.WriteLine($"  [{m}]  {zh}  ({en})");
		}
		else
		{
			// 可能是 i18n 名本身
			var owner = items.FirstOrDefault(i =>
				i.I18n.Values.Any(p => p.Name == m));
			if (owner != null)
			{
				var zh = owner.I18n.TryGetValue(Language.ZhHans, out var z) ? z.Name : "";
				var en = owner.I18n.TryGetValue(Language.En, out var e) ? e.Name : "";
				Console.Error.WriteLine($"  \"{m}\" → {zh} ({en})  slug={owner.Slug}");
			}
			else
				Console.Error.WriteLine($"  \"{m}\" (无对应物品)");
		}
	}
}
