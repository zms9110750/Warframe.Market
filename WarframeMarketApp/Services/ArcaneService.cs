using zms9110750.WarframeMarketApi.Models.Statistics;
using WarframeMarketApp.Data;

namespace WarframeMarketApp.Services;

/// <summary>
/// 赋能包计算服务：期望值 + 流动性封顶
/// </summary>
public class ArcaneService
{
	private readonly CacheService _cache;
	private readonly ItemsService _items;

	// 合成消耗（赋能用）：从 R0 合到 R1-R5 各需多少个 R0
	public static IReadOnlyList<int> SyntheticConsumption { get; } = [1, 3, 6, 10, 15, 21];

	// 一组小小黑(420荧尘) × 6组/包 / 200(每包价格) × 3(每包开3个赋能)
	public const double PackGainRate = 420.0 * 6 / 200 * 3; // = 37.8

	public ArcaneService(CacheService cache, ItemsService items)
	{
		_cache = cache;
		_items = items;
	}

	/// <summary>
	/// 计算赋能包的期望价值。
	/// </summary>
	/// <param name="pack">赋能包配置</param>
	/// <param name="purchase">每天购买组数（0=自用不算流动性）</param>
	public async Task<double> GetReferencePriceAsync(ArcanePackConfig pack, int purchase = 0)
	{
		double total = 0;
		foreach (var q in pack.Items)
		{
			foreach (var itemName in q.Items)
			{
				// 通过物品名搜索 slug
				var results = await _items.SearchAsync(itemName);
				var item = results.FirstOrDefault();
				if (item == null) continue;
				var slug = item.Slug;

				// 获取统计数据
				var stat = await _cache.GetStatisticsAsync(slug);
				if (stat == null) continue;

				// 参考价（货币价值）
				var maxPrice = _items.GetMaxReferencePrice(stat);
				if (maxPrice == null) continue;

				// 混合价：满级价 / 合成消耗
				var firstRanked = stat.Payload?.StatisticsClosed?.Day90?.FirstOrDefault(e => e.ModRank > 0);
				var rank = firstRanked?.ModRank ?? 0;
				var syn = rank > 0 && rank <= 5 ? SyntheticConsumption[rank] : 1;
				var materialPrice = maxPrice.Value / syn;

				// 有效数量：概率 × PackGainRate
				var prob = pack.GetProbability(itemName);
				var effectiveVolume = prob * PackGainRate;

				// 流动性封顶
				if (purchase > 0 && stat.Payload?.StatisticsClosed?.Day90 != null)
				{
					var dailyVolume = stat.Payload.StatisticsClosed.Day90
						.Sum(e => e.Volume * (e.ModRank is > 0 and <= 5 ? SyntheticConsumption[e.ModRank.Value] : 1)) / 90.0;
					effectiveVolume = Math.Min(effectiveVolume, dailyVolume / purchase);
				}

				total += materialPrice * effectiveVolume;
			}
		}
		return total;
	}

	/// <summary>获取物品的日均交易量（按 90 天）</summary>
	public double GetDailyVolume(Statistic? stat)
	{
		if (stat?.Payload?.StatisticsClosed?.Day90 == null) return 0;
		return stat.Payload.StatisticsClosed.Day90
			.Sum(e => e.Volume * (e.ModRank is > 0 and <= 5 ? SyntheticConsumption[e.ModRank.Value] : 1)) / 90.0;
	}
}
