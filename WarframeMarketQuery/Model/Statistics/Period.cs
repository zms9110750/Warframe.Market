using System.Text.Json.Serialization;

namespace WarframeMarketQuery.Model.Statistics;

/// <summary>
/// 表示市场数据中的统计信息项，包含名为Day90和Hour48的数据列表。
/// </summary>
/// <param name="Hour48">48小时内的数据，每2小时的跨度，UTC时间偶数小时刷新</param>
/// <param name="Day90"> 90天内的数据，每天的跨度。UTC0:00刷新</param>
public record Period(
	[property: JsonPropertyName("48hours")] Entry[] Hour48,
	[property: JsonPropertyName("90days")] Entry[] Day90);