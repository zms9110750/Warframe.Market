namespace zms9110750.WarframeMarketApi.Requests;

/// <summary>
/// 更新现有订单的请求体（仅需包含要修改的字段）
/// </summary>
/// <param name="Platinum">铂金价格，1 ~ 900000</param>
/// <param name="Quantity">订单数量，1 ~ 9999</param>
/// <param name="Visible">是否公开可见</param>
/// <param name="PerTrade">每次交易数量（仅批量交易物品可用），须能整除最终 quantity</param>
/// <param name="Rank">物品等级（仅支持等级的物品可用），0 ~ maxRank</param>
/// <param name="Charges">充能次数（仅支持充能的物品可用），0 ~ maxCharges</param>
/// <param name="Subtype">子类型（仅支持子类型的物品可用）</param>
/// <param name="AmberStars">琥珀星星数量（仅支持琥珀星的物品可用），0 ~ maxAmberStars</param>
/// <param name="CyanStars">青蓝星星数量（仅支持青蓝星的物品可用），0 ~ maxCyanStars</param>
public record PatchOrderRequest(
	int? Platinum = null,
	int? Quantity = null,
	bool? Visible = null,
	int? PerTrade = null,
	int? Rank = null,
	int? Charges = null,
	string? Subtype = null,
	int? AmberStars = null,
	int? CyanStars = null
);
