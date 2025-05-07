using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warframe.Market.Helper;
using Warframe.Market.Helper.Abstract;
using Warframe.Market.Model.Items;
using Warframe.Market.Model.LocalItems;
using Warframe.Market.Model.Statistics;

namespace Warframe.Market.Extend;
public static class StatisticExtend
{
	public static IReadOnlyList<int> SyntheticConsumption { get; } = [1, 3, 6, 10, 15, 21];
	public static IReadOnlyList<double> DefaultWeight { get; } = [40, 25, 15, 5, 5, 5, 5];
	public static double GetReferencePrice(this Statistic statistic, Func<Entry, bool>? filter = null, IEnumerable<double>? weight = null)
	{
		// 如果 filter 为 null，则默认返回 true
		filter ??= _ => true;

		// 如果 weight 为 null，则使用默认权重
		weight ??= DefaultWeight;

		// 获取过去 90 天的统计数据，并根据 filter 过滤，按时间降序排序
		var filteredEntries = statistic.Payload.StatisticsClosed.Day90
			.Where(filter)
			.OrderByDescending(x => x.Datetime)
			.Zip(weight) // 将数据与权重配对
			.ToArray();

		// 如果没有数据，返回 0
		if (filteredEntries.Length == 0)
		{
			return 0;
		}

		// 初始化总权重和加权总和
		var totalWeight = 0.0;
		var weightedSum = 0.0;

		// 遍历每个数据项，计算加权总和和总权重
		foreach (var (entry, weightValue) in filteredEntries)
		{
			var weightedVolume = entry.Volume * weightValue;
			totalWeight += weightedVolume;
			weightedSum += weightedVolume * entry.Median;
		}

		// 返回加权平均价
		return weightedSum / totalWeight;
	}
	public static double GetDefaultReferencePrice(this Statistic statistic, ItemShort itemShort, IEnumerable<double>? weight = null)
	{
		return GetReferencePrice(statistic, PriceFilterDefault(itemShort), weight);
	}
	public static double GetMaxRankReferencePrice(this Statistic statistic, ItemShort itemShort, IEnumerable<double>? weight = null)
	{
		return GetReferencePrice(statistic, PriceFilterMaxRank(itemShort), weight);
	}
	public static double GetFilterSubtypeReferencePrice(this Statistic statistic, Subtypes subtypes, IEnumerable<double>? weight = null)
	{
		return GetReferencePrice(statistic, PriceFilterSubtype(subtypes), weight);
	}
	public static double GetMaterialBasedReferencePrice(this Statistic statistic, ItemShort itemShort, IEnumerable<double>? weight = null)
	{
		return GetMaxRankReferencePrice(statistic, itemShort, weight) / SyntheticConsumption[itemShort.MaxRank ?? 0];
	}
	public static async Task<double> GetReferencePrice(this ArcanePackage package, ItemCache cache, IWMClient client, int purchase = 0, IEnumerable<double>? weight = null)
	{
		var result = package
			.SelectMany(s => s)
			.Select(async item =>
			{
				var itemshort = cache[item];
				var statistics = await client.GetStatisticsAsync(itemshort);
				double effectiveVolume = package.GetProbability(item) * ArcanePackage.PackGainRate;
				if (purchase != 0)
				{
					effectiveVolume = Math.Min(effectiveVolume, statistics.Payload.StatisticsClosed.Day90.Sum(s => s.Volume * SyntheticConsumption[s.ModRank ?? 0]) / 90 / purchase);
				}
				return statistics.GetMaterialBasedReferencePrice(itemshort, weight) * effectiveVolume;
			});
		return (await Task.WhenAll(result)).Sum();
	}

	public static Func<Entry, bool>? PriceFilterDefault(ItemShort itemShort)
	{
		return itemShort.ItemType switch
		{
			ItemType.ArcaneEnhancement => static s => s.ModRank == 0,
			ItemType.AyatanSculpture => static s => s.AmberStars != 0,
			ItemType.CraftedComponent => static s => s.Subtype == Subtypes.Blueprint,
			ItemType.MOD => static s => s.ModRank == 0,
			ItemType.Relic => static s => s.Subtype == Subtypes.Intact,
			ItemType.RivenMOD => static s => s.Subtype == Subtypes.Unrevealed,
			_ => null,
		};
	}
	public static Func<Entry, bool>? PriceFilterMaxRank(ItemShort itemShort)
	{
		return itemShort.ItemType switch
		{
			ItemType.ArcaneEnhancement => static s => s.ModRank != 0,
			ItemType.MOD => static s => s.ModRank != 0,
			_ => PriceFilterDefault(itemShort),
		};
	}
	public static Func<Entry, bool>? PriceFilterSubtype(Subtypes subtypes)
	{
		return s => s.Subtype == subtypes;
	} 
}
