namespace zms9110750.WarframeMarketApi.Models.Rivens;

/// <summary>
/// 裂罅属性信息
/// </summary>
/// <param name="Id">唯一标识符</param>
/// <param name="Slug">URL 友好名称</param>
/// <param name="GameRef">游戏内路径引用</param>
/// <param name="Group">分组类别</param>
/// <param name="Prefix">前缀名称</param>
/// <param name="Suffix">后缀名称</param>
/// <param name="ExclusiveTo">专属武器类型列表</param>
/// <param name="PositiveIsNegative">正值是否视为负值</param>
/// <param name="Unit">计量单位</param>
/// <param name="PositiveOnly">是否只能为正值</param>
/// <param name="NegativeOnly">是否只能为负值</param>
/// <param name="I18n">多语言本地化文本</param>
public record RivenAttribute(
	string Id,
	string Slug,
	string GameRef,
	string Group,
	string Prefix,
	string Suffix,
	string[]? ExclusiveTo,
	bool? PositiveIsNegative,
	string Unit,
	bool? PositiveOnly,
	bool? NegativeOnly,
	Dictionary<Items.Language, RivenAttributeI18N> I18n
)
{
	public static implicit operator string(RivenAttribute item) => item.Slug;
}
