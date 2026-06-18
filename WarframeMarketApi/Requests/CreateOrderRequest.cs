namespace zms9110750.WarframeMarketApi.Requests;

/// <summary>
/// 创建新订单的请求体
/// </summary>
/// <param name="ItemId">物品 ID，物品必须存在且可交易</param>
/// <param name="Type">订单类型（sell / buy）</param>
/// <param name="Platinum">铂金价格，1 ~ 900000</param>
/// <param name="Quantity">订单数量，1 ~ 9999</param>
/// <param name="Visible">是否公开可见，默认 false</param>
/// <param name="PerTrade">每次交易数量（批量交易物品必填），须能整除 quantity</param>
/// <param name="Rank">物品等级（支持等级的物品必填），0 ~ maxRank</param>
/// <param name="Charges">充能次数（支持充能的物品必填），0 ~ maxCharges</param>
/// <param name="Subtype">子类型（有子类型的物品必填）</param>
/// <param name="AmberStars">琥珀星星数量（支持琥珀星的物品必填），0 ~ maxAmberStars</param>
/// <param name="CyanStars">青蓝星星数量（支持青蓝星的物品必填），0 ~ maxCyanStars</param>
internal record CreateOrderRequest(
	string ItemId,
	string Type,
	int Platinum,
	int Quantity,
	bool Visible = false,
	int? PerTrade = null,
	int? Rank = null,
	int? Charges = null,
	string? Subtype = null,
	int? AmberStars = null,
	int? CyanStars = null
);
