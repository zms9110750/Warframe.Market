namespace zms9110750.WarframeMarketApi.Models.Items;

/// <summary>
/// 物品完整信息，继承 ItemShort，包含套装、稀有度、交易税等扩展字段
/// </summary>
/// <param name="Id">物品唯一标识符</param>
/// <param name="Slug">URL 友好名称</param>
/// <param name="GameRef">游戏内路径引用</param>
/// <param name="Tags">物品标签列表</param>
/// <param name="MaxRank">可达到的最大等级</param>
/// <param name="Vaulted">是否已入库</param>
/// <param name="Ducats">杜卡特价值</param>
/// <param name="MaxAmberStars">最大琥珀星星数量</param>
/// <param name="MaxCyanStars">最大青蓝星星数量</param>
/// <param name="BaseEndo">基础内融核心值</param>
/// <param name="EndoMultiplier">内融核心值倍率</param>
/// <param name="Subtypes">物品子类型列表</param>
/// <param name="I18n">多语言本地化信息</param>
/// <param name="UrlName">用于生成 URL 的名称</param>
/// <param name="Tradable">是否可交易</param>
/// <param name="SetRoot">是否为套装根物品</param>
/// <param name="SetParts">套装部件 ID 列表</param>
/// <param name="QuantityInSet">在套装中的数量</param>
/// <param name="Rarity">稀有度（字符串）</param>
/// <param name="BulkTradable">是否可批量交易</param>
/// <param name="MaxCharges">最大充能次数</param>
/// <param name="Vosfor">Vosfor 价值</param>
/// <param name="ReqMasteryRank">所需精通等级</param>
/// <param name="TradingTax">交易税</param>
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
	ItemSubtypeSet? Subtypes,
	Dictionary<Language, LanguagePake> I18n,
	string UrlName,
	bool Tradable,
	bool? SetRoot,
	HashSet<string>? SetParts,
	int? QuantityInSet,
	string? Rarity,
	bool? BulkTradable,
	int? MaxCharges,
	int? Vosfor,
	int? ReqMasteryRank,
	int? TradingTax
) : ItemShort(Id, Slug, GameRef, Tags, MaxRank, Vaulted, Ducats, MaxAmberStars, MaxCyanStars, BaseEndo, EndoMultiplier, Subtypes, I18n);
