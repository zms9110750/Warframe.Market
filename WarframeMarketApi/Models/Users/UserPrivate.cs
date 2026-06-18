namespace zms9110750.WarframeMarketApi.Models.Users;

/// <summary>
/// 已认证用户的私有资料，包含订阅、通知、邮箱等敏感信息
/// </summary>
/// <param name="Id">用户唯一标识符</param>
/// <param name="Role">用户角色</param>
/// <param name="Tier">订阅等级</param>
/// <param name="Subscription">订阅状态</param>
/// <param name="IngameName">游戏内名称</param>
/// <param name="Slug">URL 友好名称</param>
/// <param name="Avatar">头像路径</param>
/// <param name="Background">背景图路径</param>
/// <param name="About">个人简介（HTML）</param>
/// <param name="AboutRaw">个人简介（原始 Markdown）</param>
/// <param name="Reputation">声望分数</param>
/// <param name="MasteryRank">精通等级</param>
/// <param name="Credits">游戏内货币余额</param>
/// <param name="LastSeen">最后在线时间</param>
/// <param name="Platform">游戏平台</param>
/// <param name="Crossplay">是否启用跨平台交易</param>
/// <param name="Locale">偏好语言</param>
/// <param name="Theme">UI 主题偏好</param>
/// <param name="SyncLocale">是否跨设备同步语言</param>
/// <param name="SyncTheme">是否跨设备同步主题</param>
/// <param name="Verification">验证状态</param>
/// <param name="CheckCode">唯一校验码</param>
/// <param name="CreatedAt">账号创建时间</param>
/// <param name="UnreadNotifications">未读通知数</param>
/// <param name="DeleteInProgress">是否正在删除账号</param>
/// <param name="DeleteAt">计划删除日期</param>
/// <param name="HasEmail">是否有邮箱地址</param>
/// <param name="Email">邮箱地址</param>
public record UserPrivate(
	string Id,
	string Role,
	string Tier,
	bool Subscription,
	string IngameName,
	string Slug,
	string? Avatar,
	string? Background,
	string? About,
	string? AboutRaw,
	int Reputation,
	int MasteryRank,
	int? Credits,
	string LastSeen,
	string Platform,
	bool Crossplay,
	string Locale,
	string? Theme,
	bool? SyncLocale,
	bool? SyncTheme,
	bool? Verification,
	string? CheckCode,
	string? CreatedAt,
	int? UnreadNotifications,
	bool? DeleteInProgress,
	string? DeleteAt,
	bool? HasEmail,
	string? Email
);
