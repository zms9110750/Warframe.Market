namespace WarframeMarketApp.Data;

/// <summary>
/// 缓存的物品基类（ItemShort 的字段）
/// </summary>
public class CachedItemBase
{
	public string Id { get; set; } = "";
	public string Slug { get; set; } = "";
	public string GameRef { get; set; } = "";
	public string TagsJson { get; set; } = "[]";
	public int? MaxRank { get; set; }
	public bool? Vaulted { get; set; }
	public int? Ducats { get; set; }
	public int? MaxAmberStars { get; set; }
	public int? MaxCyanStars { get; set; }
	public int? BaseEndo { get; set; }
	public double? EndoMultiplier { get; set; }
	public string SubtypesJson { get; set; } = "[]";
	public string SetPartsJson { get; set; } = "[]";
}

/// <summary>
/// 缓存的物品完整信息（Item 比 ItemShort 多出的字段）
/// </summary>
public class CachedItemDetail : CachedItemBase
{
	public string UrlName { get; set; } = "";
	public bool Tradable { get; set; }
	public bool? SetRoot { get; set; }
	public int? QuantityInSet { get; set; }
	public string? Rarity { get; set; }
	public bool? BulkTradable { get; set; }
	public int? MaxCharges { get; set; }
	public int? Vosfor { get; set; }
	public int? ReqMasteryRank { get; set; }
	public int? TradingTax { get; set; }
}
