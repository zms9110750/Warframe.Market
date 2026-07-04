namespace WarframeMarketQuery.Model.Statistics;

/// <summary>
/// 表示整个市场数据负载的根对象。
/// </summary>
/// <param name="StatisticsClosed"> 已完成的订单统计数据。 </param>
/// <param name="StatisticsLive"> 活动中的订单统计数据。 </param>
public record Payload(
	  Period StatisticsClosed,
	  Period StatisticsLive);