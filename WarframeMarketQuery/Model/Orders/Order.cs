using WarframeMarketQuery.Model.Items;
using WarframeMarketQuery.Model.Users;

namespace WarframeMarketQuery.Model.Orders;
/// <summary>
/// 订单记录类，表示游戏交易平台上的交易订单信息
/// </summary>
/// <param name="Id">订单唯一标识符</param>
/// <param name="Type">订单类型（"buy"或"sell"）</param>
/// <param name="Platinum">订单涉及的铂金货币总量</param>
/// <param name="Quantity">订单包含的商品数量</param>
/// <param name="PerTrade">（可选）每次交易的商品数量</param>
/// <param name="Rank">（可选）订单中物品的等级或级别</param>
/// <param name="Charges">（可选）剩余充能次数（用于requiem模组）</param>
/// <param name="Subtype">（可选）物品的具体子类型或分类</param>
/// <param name="AmberStars">（可选）雕塑订单中的琥珀星数量</param>
/// <param name="CyanStars">（可选）雕塑订单中的青色星数量</param>
/// <param name="Visible">订单是否对公众可见</param>
/// <param name="CreatedAt">订单创建时间</param>
/// <param name="UpdatedAt">订单最后修改时间</param>
/// <param name="ItemId">订单涉及物品的唯一标识符</param>
/// <param name="Group">订单所属的用户自定义分组</param>
/// <param name="User">订单所属的用户</param>
public record Order(
	string Id,
	OrderType Type,
	int Platinum,
	int Quantity,
	byte? PerTrade,
	byte? Rank,
	byte? Charges,
	ItemSubtypes? Subtype,
	byte? AmberStars,
	byte? CyanStars,
	bool Visible,
	string CreatedAt,
	string UpdatedAt,
	string ItemId,
	string Group,
	User? User
)
{
	public static implicit operator string(Order item)
	{
		return item.ItemId;
	}
}