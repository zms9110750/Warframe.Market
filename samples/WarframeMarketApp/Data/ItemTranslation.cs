using zms9110750.WarframeMarketApi.Models.Items;

namespace WarframeMarketApp.Data;

/// <summary>
/// 物品翻译。（ItemId, Language）联合主键。
/// 继承 <see cref="LanguagePake"/>，EF 自动追踪关联。
/// </summary>
public record ItemTranslation(
	string ItemId,
	string Language,
	string Name,
	string? Description,
	string? WikiLink,
	string Icon,
	string Thumb,
	string? SubIcon
) : LanguagePake(Name, Description, WikiLink, Icon, Thumb, SubIcon)
{
	/// <summary>导航属性：所属物品</summary>
	public ItemShort? Item { get; set; }
}
