using WarframeMarketQuery.Model.Items;

namespace WarframeMarketQuery.Model.Statistics;

/// <summary>
/// 表示市场数据中的一个数据条目，包含交易的各种统计信息。
/// </summary>
/// <param name="Datetime"> 数据条目的时间戳。 </param>
/// <param name="Volume"> 交易量。 </param>
/// <param name="MinPrice"> 最低交易价格。 </param>
/// <param name="MaxPrice"> 最高交易价格。 </param>
/// <param name="AvgPrice"> 平均交易价格。 </param>
/// <param name="WaPrice"> 加权平均价格（可能根据特定算法计算）。 </param>
/// <param name="Median"> 中位数价格。 </param>
/// <param name="OrderType"> 订单类型（买或卖）。 </param>
/// <param name="Id"> 虚拟订单的ID。 </param>
/// <param name="ModRank"> MOD等级（可选）。 </param>
/// <param name="Subtype">物品子类型（可选）</param>
/// <param name="AmberStars">琥珀星数量（可选）</param>
/// <param name="CyanStars">青星数量（可选）</param>
/// <param name="OpenPrice"> 开盘价格（仅当OrderType为"sell"且数据包含开盘信息时适用）。 </param>
/// <param name="ClosedPrice"> 收盘价格（仅当OrderType为"sell"且数据包含收盘信息时适用）。 </param>
/// <param name="DonchTop"> Donchian通道上界（仅当数据包含此信息时适用）。 </param>
/// <param name="DonchBot"> Donchian通道下界（仅当数据包含此信息时适用）。 </param>
public record Entry(
	DateTime Datetime,
	int Volume,
	float MinPrice,
	float MaxPrice,
	float AvgPrice,
	float WaPrice,
	float Median,
	string? OrderType,
	string Id,
	int? ModRank,
	ItemSubtypes? Subtype,
	sbyte? AmberStars,
	sbyte? CyanStars,
	float? OpenPrice,
	float? ClosedPrice,
	float? DonchTop,
	float? DonchBot);

