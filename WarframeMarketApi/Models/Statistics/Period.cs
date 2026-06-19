using System.Text.Json;
using System.Text.Json.Serialization;

namespace zms9110750.WarframeMarketApi.Models.Statistics;

/// <summary>
/// 统计时段数据，包含 48 小时和 90 天两个粒度
/// </summary>
/// <param name="Hour48">48 小时内数据，每 2h 跨度</param>
/// <param name="Day90">90 天内数据，每天跨度</param>
public record Period(
	[property: JsonPropertyName("48hours")] Entry[] Hour48,
	[property: JsonPropertyName("90days")] Entry[] Day90
);
