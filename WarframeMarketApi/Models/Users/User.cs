using System.Globalization;

namespace zms9110750.WarframeMarketApi.Models.Users;

/// <summary>
/// 用户公开资料
/// </summary>
/// <param name="Id">用户唯一标识符</param>
/// <param name="Role">用户角色</param>
/// <param name="Tier">订阅/支持等级</param>
/// <param name="IngameName">游戏内名称</param>
/// <param name="Slug">URL 友好名称</param>
/// <param name="Avatar">头像路径</param>
/// <param name="Background">个人背景图路径</param>
/// <param name="About">个人简介（HTML）</param>
/// <param name="Reputation">声望分数</param>
/// <param name="MasteryRank">精通等级</param>
/// <param name="Status">在线状态</param>
/// <param name="Activity">当前活动</param>
/// <param name="LastSeen">最后在线时间（ISO 8601）</param>
/// <param name="Platform">游戏平台</param>
/// <param name="Crossplay">是否启用跨平台交易</param>
/// <param name="Locale">偏好语言</param>
/// <param name="Banned">是否被封禁</param>
/// <param name="BanUntil">封禁到期时间（ISO 8601）</param>
public record User(
	string Id,
	string Role,
	string Tier,
	string IngameName,
	string Slug,
	string? Avatar,
	string? Background,
	string? About,
	int Reputation,
	int MasteryRank,
	string Status,
	Activity? Activity,
	string LastSeen,
	string Platform,
	bool Crossplay,
	string Locale,
	bool? Banned,
	string? BanUntil
)
{
	/// <summary>最后在线时间（本地时间）</summary>
	public DateTime LastSeenLocal => DateTime.Parse(LastSeen, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToLocalTime();
	/// <summary>封禁到期时间（本地时间）</summary>
	public DateTime? BanUntilLocal => BanUntil != null
		? DateTime.Parse(BanUntil, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToLocalTime()
		: null;

	public static implicit operator string(User item) => item.Slug;
}
