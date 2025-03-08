using System.Text.Json.Serialization;
using Newtonsoft.Json;
namespace Warframe.Market.Model.Statistics;

/// <summary>
/// 表示整个市场数据负载的根对象。
/// </summary>
/// <param name="StatisticsClosed"> 已完成的订单统计数据。 </param>
/// <param name="StatisticsLive"> 活动中的订单统计数据。 </param>
public record MarketDataPayload(
	  [property: JsonPropertyName("statistics_closed"), JsonProperty("statistics_closed")] StatisticsPeriod StatisticsClosed
	, [property: JsonPropertyName("statistics_live"), JsonProperty("statistics_live")] StatisticsPeriod StatisticsLive)
{
	public IEnumerable<(int?, double)> GetReferencePrice(IEnumerable<double>? weight = null)
	{
		weight = weight ?? DefaultWeight;
		if (StatisticsClosed.Day90.Length == 0)
		{
			yield break;
		}
		foreach (var item in StatisticsClosed.Day90.GroupBy(x => x.ModRank).OrderBy(x => x.Key))
		{
			var 总权重 = 0.0;
			var 总和 = 0.0;
			foreach (var entry in item.OrderByDescending(x => x.Datetime).Zip(weight))
			{
				var a = entry.First.Volume * entry.Second;
				总权重 += a;
				总和 += a * entry.First.Median;
			}
			yield return (item.Key, 总和 / 总权重);
		}
	}
	public static IEnumerable<double> DefaultWeight { get; } = [40, 25, 15, 5, 5, 5, 5];
}
