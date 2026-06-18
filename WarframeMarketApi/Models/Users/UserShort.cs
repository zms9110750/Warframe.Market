namespace zms9110750.WarframeMarketApi.Models.Users;

/// <summary>
/// 嵌入在 Order / Transaction 等对象中的简短用户信息
/// </summary>
/// <param name="Id">用户唯一标识符</param>
/// <param name="IngameName">游戏内名称</param>
/// <param name="Slug">URL 友好名称</param>
/// <param name="Avatar">头像路径</param>
/// <param name="Reputation">声望分数</param>
/// <param name="Platform">游戏平台</param>
/// <param name="Crossplay">是否启用跨平台交易</param>
/// <param name="Locale">偏好语言</param>
/// <param name="Status">在线状态</param>
/// <param name="Activity">当前活动</param>
/// <param name="LastSeen">最后在线时间</param>
public record UserShort(
	string Id,
	string IngameName,
	string Slug,
	string? Avatar,
	int Reputation,
	string Platform,
	bool Crossplay,
	string Locale,
	string Status,
	Activity? Activity,
	string LastSeen
)
{
	public static implicit operator string(UserShort item) => item.Slug;
}
