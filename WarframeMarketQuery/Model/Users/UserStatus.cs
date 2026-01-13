namespace WarframeMarketQuery.Model.Users;

/// <summary>
/// 用户在线状态
/// </summary>
public enum UserStatus
{
	/// <summary>
	/// 隐身状态
	/// </summary>
	Invisible,

	/// <summary>
	/// 离线状态
	/// </summary>
	Offline,

	/// <summary>
	/// 在线状态
	/// </summary>
	Online,

	/// <summary>
	/// 游戏中
	/// </summary>
	Ingame
}
