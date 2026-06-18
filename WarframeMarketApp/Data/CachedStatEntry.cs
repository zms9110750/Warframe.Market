namespace WarframeMarketApp.Data;

/// <summary>
/// 缓存的订单统计数据条目
/// </summary>
public class CachedStatEntry
{
	public string Id { get; set; } = "";
	public string ItemId { get; set; } = "";
	public string ItemSlug { get; set; } = "";
	public DateTime Datetime { get; set; }
	public int Volume { get; set; }
	public double MinPrice { get; set; }
	public double MaxPrice { get; set; }
	public double AvgPrice { get; set; }
	public double WaPrice { get; set; }
	public double Median { get; set; }
	public string? OrderType { get; set; }
	public int? ModRank { get; set; }
	public string? Subtype { get; set; }
}
