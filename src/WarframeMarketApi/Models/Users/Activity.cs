using System.Globalization;

namespace zms9110750.WarframeMarketApi.Models.Users;

/// <summary>
/// 用户活动信息
/// </summary>
/// <param name="Type">活动类型（on_mission / dojo / unknown 等）</param>
/// <param name="Details">活动详情描述</param>
/// <param name="StartedAt">活动开始时间（ISO 8601）</param>
public record Activity(
	string? Type,
	string? Details,
	string? StartedAt
)
{
	/// <summary>活动开始时间（本地时间）</summary>
	public DateTime? StartedAtLocal => StartedAt != null
		? DateTime.Parse(StartedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToLocalTime()
		: null;
}
