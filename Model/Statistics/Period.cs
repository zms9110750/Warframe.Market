namespace Warframe.Market.Model.Statistics;

/// <summary>
/// 表示市场数据中的统计信息项，包含名为Day90和Hour48的数据列表（实际为90天和48小时的数据）。
/// 注意：命名可能会引起混淆，因为Day90和Hour48并不直接表示时间段。
/// </summary>
/// <param name="Hour48"> 48小时内的数据列表（实际为时间段内的数据集合）。 </param>
/// <param name="Day90"> 90天内的数据列表（实际为时间段内的数据集合）。 </param>
public record Period(
	[property: JsonProperty("48hours"), JsonPropertyName("48hours")] Entry[] Hour48
	, [property: JsonProperty("90days"), JsonPropertyName("90days")] Entry[] Day90);