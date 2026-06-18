namespace zms9110750.WarframeMarketApi.Models.Orders;

/// <summary>
/// 订单记录，包含铂金、数量、等级、用户等交易信息
/// </summary>
/// <param name="Id">订单唯一标识符</param>
/// <param name="Type">订单类型（buy / sell）</param>
/// <param name="Platinum">铂金总量</param>
/// <param name="Quantity">物品数量</param>
/// <param name="PerTrade">每次交易物品数量</param>
/// <param name="Subtype">物品子类型</param>
/// <param name="Rank">物品等级</param>
/// <param name="Charges">剩余充能次数</param>
/// <param name="AmberStars">琥珀星星数量</param>
/// <param name="CyanStars">青蓝星星数量</param>
/// <param name="Visible">是否公开可见</param>
/// <param name="CreatedAt">创建时间</param>
/// <param name="UpdatedAt">最后修改时间</param>
/// <param name="ItemId">物品标识符</param>
/// <param name="GroupId">用户自定义分组 ID</param>
/// <param name="User">创建订单的用户</param>
public record Order(
	string Id,
	string Type,
	int Platinum,
	int Quantity,
	int? PerTrade,
	string? Subtype,
	int? Rank,
	int? Charges,
	int? AmberStars,
	int? CyanStars,
	bool Visible,
	string CreatedAt,
	string UpdatedAt,
	string? ItemId,
	string? GroupId,
	Users.UserShort? User
);
