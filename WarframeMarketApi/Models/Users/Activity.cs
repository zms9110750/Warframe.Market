namespace zms9110750.WarframeMarketApi.Models.Users;

/// <summary>
/// 用户活动信息
/// </summary>
/// <param name="Type">活动类型（on_mission / dojo / unknown 等）</param>
/// <param name="Details">活动详情描述</param>
/// <param name="StartedAt">活动开始时间</param>
public record Activity(
	string? Type,
	string? Details,
	string? StartedAt
);
