namespace zms9110750.WarframeMarketApi.Models.Rivens;

/// <summary>
/// 完整裂罅武器信息
/// </summary>
/// <param name="Id">唯一标识符</param>
/// <param name="Slug">URL 友好名称</param>
/// <param name="GameRef">游戏内路径引用</param>
/// <param name="Group">分组类别</param>
/// <param name="RivenType">武器类型（kitgun / melee / pistol / rifle / shotgun / zaw）</param>
/// <param name="Disposition">裂罅倾向值</param>
/// <param name="ReqMasteryRank">所需精通等级</param>
/// <param name="I18n">多语言本地化信息</param>
public record Riven(
	string Id,
	string Slug,
	string GameRef,
	string Group,
	string RivenType,
	double Disposition,
	int ReqMasteryRank,
	Dictionary<Items.Language, RivenI18N> I18n
);
