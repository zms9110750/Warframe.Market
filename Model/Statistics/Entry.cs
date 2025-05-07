using Warframe.Market.Model.Items;

namespace Warframe.Market.Model.Statistics;

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
/// <param name="Id"> 关联物品的唯一标识符。 </param>
/// <param name="ModRank"> MOD等级（可选）。 </param>
/// <param name="Subtype">物品子类型（可选）</param>
/// <param name="AmberStars">琥珀星数量（可选）</param>
/// <param name="CyanStars">青星数量（可选）</param>
/// <param name="OpenPrice"> 开盘价格（仅当OrderType为"sell"且数据包含开盘信息时适用）。 </param>
/// <param name="ClosedPrice"> 收盘价格（仅当OrderType为"sell"且数据包含收盘信息时适用）。 </param>
/// <param name="DonchTop"> Donchian通道上界（仅当数据包含此信息时适用）。 </param>
/// <param name="DonchBot"> Donchian通道下界（仅当数据包含此信息时适用）。 </param>
public record Entry(
	[property: JsonPropertyName("datetime"), JsonProperty("datetime")] DateTime Datetime
	, [property: JsonPropertyName("volume"), JsonProperty("volume")] int Volume
	, [property: JsonPropertyName("min_price"), JsonProperty("min_price")] float MinPrice
	, [property: JsonPropertyName("max_price"), JsonProperty("max_price")] float MaxPrice
	, [property: JsonPropertyName("avg_price"), JsonProperty("avg_price")] float AvgPrice
	, [property: JsonPropertyName("wa_price"), JsonProperty("wa_price")] float WaPrice
	, [property: JsonPropertyName("median"), JsonProperty("median")] float Median
	, [property: JsonPropertyName("order_type"), JsonProperty("order_type")] string? OrderType
	, [property: JsonPropertyName("id"), JsonProperty("id")] string Id
	, [property: JsonPropertyName("mod_rank"), JsonProperty("mod_rank")] int? ModRank
	, [property: JsonPropertyName("subtype"), JsonProperty("subtype")] Subtypes? Subtype
	, [property: JsonPropertyName("amberStars"), JsonProperty("amberStars")] sbyte? AmberStars
	, [property: JsonPropertyName("cyanStars"), JsonProperty("cyanStars")] sbyte? CyanStars
	, [property: JsonPropertyName("open_price"), JsonProperty("open_price")] float? OpenPrice
	, [property: JsonPropertyName("closed_price"), JsonProperty("closed_price")] float? ClosedPrice
	, [property: JsonPropertyName("donch_top"), JsonProperty("donch_top")] float? DonchTop
	, [property: JsonPropertyName("donch_bot"), JsonProperty("donch_bot")] float? DonchBot);

