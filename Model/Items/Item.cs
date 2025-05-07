using Warframe.Market.Model.LocalItems;

namespace Warframe.Market.Model.Items;

/// <summary>
/// 游戏中的物品信息
/// </summary>
/// <param name="UrlName">物品的URL名称，用于生成URL</param>
/// <param name="Tradable">物品是否可交易</param>
/// <param name="SetRoot">物品是否为套装根物品</param>
/// <param name="SetParts">物品的套装部件列表</param>
/// <param name="QuantityInSet">物品在套装中的数量</param>
/// <param name="Rarity">物品的稀有度</param>
/// <param name="ReqMasteryRank">物品所需的精通等级</param>
/// <param name="TradingTax">物品的交易税</param>
public record Item(
	string Id,
	string Slug,
	string GameRef,
	HashSet<string> Tags,
	int? MaxRank,
	bool? Vaulted,
	int? Ducats,
	int? MaxAmberStars,
	int? MaxCyanStars,
	int? BaseEndo,
	float? EndoMultiplier,
	HashSet<Subtypes>? Subtypes,
	[property: JsonPropertyName("urlName"), JsonProperty("urlName")] string UrlName,
	[property: JsonPropertyName("tradable"), JsonProperty("tradable")] bool Tradable,
	[property: JsonPropertyName("setRoot"), JsonProperty("setRoot")] bool? SetRoot,
	[property: JsonPropertyName("setParts"), JsonProperty("setParts")] HashSet<string>? SetParts,
	[property: JsonPropertyName("quantityInSet"), JsonProperty("quantityInSet")] int? QuantityInSet,
	[property: JsonPropertyName("rarity"), JsonProperty("rarity")] Subtypes? Rarity,
	[property: JsonPropertyName("reqMasteryRank"), JsonProperty("reqMasteryRank")] int? ReqMasteryRank,
	[property: JsonPropertyName("tradingTax"), JsonProperty("tradingTax")] int? TradingTax
) : ItemShort(Id, Slug, GameRef, Tags, MaxRank, Vaulted, Ducats, MaxAmberStars, MaxCyanStars, BaseEndo, EndoMultiplier, Subtypes);
