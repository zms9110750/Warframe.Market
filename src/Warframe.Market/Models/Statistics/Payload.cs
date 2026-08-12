namespace zms9110750.WarframeMarketApi.Models.Statistics;

/// <summary>
/// 统计数据负载，包含已关闭和活动中的订单统计
/// </summary>
/// <param name="StatisticsClosed">已完成的订单统计数据</param>
/// <param name="StatisticsLive">活动中的订单统计数据</param>
public record Payload(
    Period StatisticsClosed,
    Period StatisticsLive
);
