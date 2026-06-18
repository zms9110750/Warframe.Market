namespace zms9110750.WarframeMarketApi.Models.Items;

/// <summary>
/// 物品的简要信息，用于物品列表
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
public record ItemShort(
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
	Dictionary<Language, LanguagePake> I18n
);
