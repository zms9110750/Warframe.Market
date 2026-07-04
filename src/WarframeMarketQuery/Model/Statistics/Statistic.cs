namespace WarframeMarketQuery.Model.Statistics;
/// <summary>
/// 统计数据
/// </summary>
/// <param name="Payload">负载</param>
public record Statistic(Payload Payload)
{
	public static implicit operator Statistic(Response<Statistic> item)
	{
		return item.Data;
	}
}