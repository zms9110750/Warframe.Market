using System.Globalization;

namespace zms9110750.WarframeMarketApi.Models.Achievements;

/// <summary>
/// 成就进度状态（仅用户特定成就时存在）
/// </summary>
/// <param name="Featured">是否精选</param>
/// <param name="Hidden">是否对公众隐藏</param>
/// <param name="Progress">当前进度</param>
/// <param name="CompletedAt">完成时间（ISO 8601）</param>
public record AchievementState(
	bool Featured,
	bool Hidden,
	int? Progress,
	string? CompletedAt
)
{
	/// <summary>完成时间（本地时间）</summary>
	public DateTime? CompletedAtLocal => CompletedAt != null
		? DateTime.Parse(CompletedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToLocalTime()
		: null;
}
