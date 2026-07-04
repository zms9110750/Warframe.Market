namespace zms9110750.WarframeMarketApi.Models.Users;

/// <summary>
/// 访问权限范围
/// </summary>
public enum Scope
{
	/// <summary>全部</summary>
	All,
	/// <summary>订单</summary>
	Orders,
	/// <summary>合同</summary>
	Contracts,
	/// <summary>聊天</summary>
	Chats,
	/// <summary>背包</summary>
	Inventory,
	/// <summary>高级</summary>
	Advance,
}
