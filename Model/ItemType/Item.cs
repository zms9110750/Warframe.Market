using System;
using System.Linq;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warframe.Market.Helper;
using Warframe.Market.Model.Statistics;
namespace Warframe.Market.Model.ItemType;
/// <summary>
/// 基础物品类型
/// </summary>
/// <param name="Id">物品的唯一标识符</param>
/// <param name="UrlName">物品的URL名称，用于生成URL</param>
/// <param name="Slug">物品的Slug，用于唯一标识物品</param>
/// <param name="GameRef">物品所属游戏的引用</param>
/// <param name="Tags">物品的标签列表</param>
/// <param name="TradingTax">物品的交易税</param>
/// <param name="I18n">物品的多语言信息，键为语言代码，值为对应的翻译信息</param>
public record Item(
	[property: JsonPropertyName("id"), JsonProperty("id")] string Id,
	[property: JsonPropertyName("urlName"), JsonProperty("urlName")] string UrlName,
	[property: JsonPropertyName("slug"), JsonProperty("slug")] string Slug,
	[property: JsonPropertyName("gameRef"), JsonProperty("gameRef")] string GameRef,
	[property: JsonPropertyName("tags"), JsonProperty("tags")] HashSet<string> Tags,
	[property: JsonPropertyName("tradingTax"), JsonProperty("tradingTax")] int TradingTax,
	[property: JsonPropertyName("i18n"), JsonProperty("i18n")] Dictionary<string, ItemI18n> I18n)
{
	protected MarketData? Statistics { get; set; }
	public async ValueTask<double> GetReferencePrice(WMClient client, Func<MarketDataEntry, bool>? filter = null, IEnumerable<double>? weight = null)
	{
		filter ??= _ => true;
		weight ??= DefaultWeight;
		if (Statistics == null)
		{
			Statistics = await client.GetStatisticsAsync(Slug);
		}
		var day90 = Statistics.Payload.StatisticsClosed.Day90.Where(filter).OrderByDescending(x => x.Datetime).Zip(weight).ToArray();
		if (day90.Length == 0)
		{
			return 0;
		}
		var 总权重 = 0.0;
		var 总和 = 0.0;
		foreach (var entry in day90)
		{
			var a = entry.First.Volume * entry.Second;
			总权重 += a;
			总和 += a * entry.First.Median;
		}
		return 总和 / 总权重;
	}
	public static IEnumerable<double> DefaultWeight { get; } = [40, 25, 15, 5, 5, 5, 5];

	protected ValueTask<double> GetReferencePrice<TSubtypes>(WMClient client, TSubtypes subtypes, IEnumerable<double>? weight = null) where TSubtypes : struct, Enum
	{
		return GetReferencePrice(client, s => s.Subtype == subtypes.ToString(), weight);
	}
}
