namespace zms9110750.WarframeMarketApi.Models.Orders;

/// <summary>
/// 交易记录的物品明细
/// </summary>
/// <param name="Id">物品标识符</param>
/// <param name="Rank">物品等级</param>
/// <param name="Charges">充能次数</param>
/// <param name="Subtype">物品子类型</param>
/// <param name="AmberStars">琥珀星星数量</param>
/// <param name="CyanStars">青蓝星星数量</param>
public record TxItem(
	string Id,
	int? Rank,
	int? Charges,
	string? Subtype,
	int? AmberStars,
	int? CyanStars
);
