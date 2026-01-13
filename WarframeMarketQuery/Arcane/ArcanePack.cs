using System.Collections;
using WarframeMarketQuery.Model.Items;

namespace WarframeMarketQuery.Arcane;


public record ArcanePack(string Name, QualityGroup[] Items) : ILookup<ItemSubtypes, string>
{
	Dictionary<ItemSubtypes, QualityGroup> QualityGroup => field ??= Items.ToDictionary(s => s.Subtypes, s => s);
	Dictionary<string, QualityGroup> QualityByItems => field ??=
					QualityGroup.SelectMany(s => s.Value.Items, (a, b) => (b, a.Value))
					.ToDictionary();

	public IEnumerable<string> this[ItemSubtypes key] => QualityGroup[key].Items;

	public int Count => QualityGroup.Count;

	public bool Contains(ItemSubtypes key) => QualityGroup.ContainsKey(key);

	/// <summary>
	/// 获取一个品质的概率综合
	/// </summary>
	/// <param name="subtype">品质</param>
	/// <returns>品质出现概率</returns>
	public double GetProbability(ItemSubtypes subtype)
	{
		return QualityGroup[subtype].Quality;
	}
	/// <summary>
	/// 获取一个道具在这个包中的出现率
	/// </summary>
	/// <param name="itemName">道具名</param>
	/// <returns>道具出现概率</returns>
	public double GetProbability(string itemName)
	{
		return QualityByItems.GetValueOrDefault(itemName)?.QualityEach ?? 0;
	}

	public IEnumerator<IGrouping<ItemSubtypes, string>> GetEnumerator()
	{
		return QualityGroup.Values.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
