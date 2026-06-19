namespace WarframeMarketApp.Pages;

/// <summary>
/// 用户搜索结果包装
/// </summary>
public class UserSearchResult
{
	public bool NotFound { get; set; }
	public bool Loading { get; set; }
	public bool LoadingPrices { get; set; }
	public string? Error { get; set; }
	public zms9110750.WarframeMarketApi.Models.Users.User? User { get; set; }
	public List<zms9110750.WarframeMarketApi.Models.Orders.Order>? Orders { get; set; }
	public Dictionary<string, zms9110750.WarframeMarketApi.Models.Items.ItemShort?> ItemCache { get; set; } = new();
	public Dictionary<string, zms9110750.WarframeMarketApi.Models.Statistics.Statistic?> Prices { get; set; } = new();
}
