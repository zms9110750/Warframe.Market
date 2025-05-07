using Autofac;
using System.Collections;
using System.Diagnostics;
using Warframe.Market.Extend;
using Warframe.Market.Model.Items;
using Warframe.Market.Model.LocalItems;
using Warframe.Market.Model.Statistics;

namespace Warframe.Market.Helper;
public class ArcanePackage(string packageName) : ILookup<Subtypes, string>
{
	public const double PackGainRate = 420.0 * 6 / 200 * 3;
	public string Name { get; } = packageName;
	public ItemCache? ItemCache { get; set; }
	public IEnumerable<string> this[Subtypes key] => QualityToItems.GetValueOrDefault(key) ?? [];
	Dictionary<Subtypes, HashSet<string>> QualityToItems { get; } = [];
	Dictionary<Subtypes, double> Quality { get; } = [];
	Dictionary<string, Subtypes> QualityByItems { get; } = [];
	public int Count => QualityToItems.Count;
	public void Add(Subtypes subtype, double quality, IEnumerable<string> strings)
	{
		QualityToItems.Add(subtype, [.. strings]);
		Quality.Add(subtype, quality);
		foreach (var item in strings)
		{
			QualityByItems.Add(item, subtype);
		}
	}
	/// <summary>
	/// 获取一个品质的概率综合
	/// </summary>
	/// <param name="subtype">品质</param>
	/// <returns>品质出现概率</returns>
	public double GetProbability(Subtypes subtype)
	{
		return Quality.GetValueOrDefault(subtype);
	}
	/// <summary>
	/// 获取一个道具在这个包中的出现率
	/// </summary>
	/// <param name="itemName">道具名</param>
	/// <returns>道具出现概率</returns>
	public double GetProbability(string itemName)
	{
		var subtype = QualityByItems.GetValueOrDefault(itemName);
		return subtype == default ? 0 : GetProbability(subtype) / QualityToItems[subtype].Count;
	}
	public IEnumerator<IGrouping<Subtypes, string>> GetEnumerator()
	{
		return QualityToItems.Select(s => (IGrouping<Subtypes, string>)new Group(s.Key, s.Value)).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool Contains(Subtypes key)
	{
		return QualityToItems.ContainsKey(key);
	}
	class Group(Subtypes key, IEnumerable<string> strings) : IGrouping<Subtypes, string>
	{
		public Subtypes Key { get; } = key;
		IEnumerator<string> Enumerator { get; } = strings.GetEnumerator();

		public IEnumerator<string> GetEnumerator()
		{
			return Enumerator;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
