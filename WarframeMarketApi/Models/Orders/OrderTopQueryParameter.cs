namespace zms9110750.WarframeMarketApi.Models.Orders;

/// <summary>
/// 查询在线玩家 Top 订单的可选查询参数
/// </summary>
/// <param name="Rank">精确等级匹配</param>
/// <param name="RankLt">等级上限（存在时忽略 Rank）</param>
/// <param name="Charges">精确充能次数匹配</param>
/// <param name="ChargesLt">充能次数上限（存在时忽略 Charges）</param>
/// <param name="AmberStars">精确琥珀星星数量匹配</param>
/// <param name="AmberStarsLt">琥珀星星数量上限（存在时忽略 AmberStars）</param>
/// <param name="CyanStars">精确青蓝星星数量匹配</param>
/// <param name="CyanStarsLt">青蓝星星数量上限（存在时忽略 CyanStars）</param>
/// <param name="Subtype">物品子类型（字符串）</param>
public record OrderTopQueryParameter(
	int? Rank,
	int? RankLt,
	int? Charges,
	int? ChargesLt,
	int? AmberStars,
	int? AmberStarsLt,
	int? CyanStars,
	int? CyanStarsLt,
	string? Subtype
);
