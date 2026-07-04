namespace zms9110750.WarframeMarketApi.Models.Locations;

/// <summary>
/// 位置节点信息
/// </summary>
/// <param name="Id">唯一标识符</param>
/// <param name="Slug">URL 友好名称</param>
/// <param name="GameRef">游戏内路径引用</param>
/// <param name="Faction">所属派系</param>
/// <param name="MinLevel">最低等级</param>
/// <param name="MaxLevel">最高等级</param>
/// <param name="I18n">多语言本地化信息</param>
public record Location(
	string Id,
	string Slug,
	string GameRef,
	string Faction,
	int MinLevel,
	int MaxLevel,
	Dictionary<Items.Language, LocationI18N> I18n
)
{
	public static implicit operator string(Location item) => item.Slug;
}
