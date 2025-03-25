namespace Warframe.Market.Model.Statistics;
public record Statistic([property: JsonPropertyName("payload"), JsonProperty("payload")] Payload Payload)
{
	public static IEnumerable<double> DefaultWeight { get; } = [40, 25, 15, 5, 5, 5, 5];
	public double GetReferencePrice(Func<Entry, bool>? filter = null, IEnumerable<double>? weight = null)
	{
		// 如果 filter 为 null，则默认返回 true
		filter ??= _ => true;

		// 如果 weight 为 null，则使用默认权重
		weight ??= DefaultWeight;

		// 获取过去 90 天的统计数据，并根据 filter 过滤，按时间降序排序
		var filteredEntries = Payload.StatisticsClosed.Day90
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
}