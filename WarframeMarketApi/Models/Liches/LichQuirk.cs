namespace zms9110750.WarframeMarketApi.Models.Liches;

/// <summary>
/// 巫妖怪癖信息
/// </summary>
/// <param name="Id">唯一标识符</param>
/// <param name="Slug">URL 友好名称</param>
/// <param name="Group">分组类别</param>
/// <param name="I18n">多语言本地化信息</param>
public record LichQuirk(
	string Id,
	string Slug,
	string Group,
	Dictionary<Items.Language, LichQuirkI18N> I18n
);
