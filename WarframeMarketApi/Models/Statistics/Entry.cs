namespace zms9110750.WarframeMarketApi.Models.Statistics;

/// <summary>
/// 统计数据条目
/// </summary>
/// <param name="Datetime">时间戳</param>
/// <param name="Volume">交易量</param>
/// <param name="MinPrice">最低价格</param>
/// <param name="MaxPrice">最高价格</param>
/// <param name="AvgPrice">平均价格</param>
/// <param name="WaPrice">加权平均价格</param>
/// <param name="Median">中位数价格</param>
/// <param name="OrderType">订单类型</param>
/// <param name="Id">虚拟订单 ID</param>
/// <param name="ModRank">MOD 等级</param>
/// <param name="Subtype">物品子类型（字符串）</param>
/// <param name="AmberStars">琥珀星星数量</param>
/// <param name="CyanStars">青蓝星星数量</param>
/// <param name="OpenPrice">开盘价格</param>
/// <param name="ClosedPrice">收盘价格</param>
/// <param name="DonchTop">Donchian 通道上界</param>
/// <param name="DonchBot">Donchian 通道下界</param>
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
	string? Subtype,
	sbyte? AmberStars,
	sbyte? CyanStars,
	float? OpenPrice,
	float? ClosedPrice,
	float? DonchTop,
	float? DonchBot
);
