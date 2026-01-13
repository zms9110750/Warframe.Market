#pragma warning disable CS1573 // 参数在 XML 注释中没有匹配的 param 标记(但其他参数有)

namespace WarframeMarketQuery.Model.Items;
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
///<inheritdoc cref = "ItemShort" /> 
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
	HashSet<ItemSubtypes>? Subtypes,
	string UrlName,
	bool Tradable,
	bool? SetRoot,
	HashSet<string>? SetParts,
	int? QuantityInSet,
	ItemSubtypes? Rarity,
	int? ReqMasteryRank,
	int? TradingTax,
	Dictionary<Language, LanguagePake> I18n
) : ItemShort(Id, Slug, GameRef, Tags, MaxRank, Vaulted, Ducats, MaxAmberStars, MaxCyanStars, BaseEndo, EndoMultiplier, Subtypes, I18n);
