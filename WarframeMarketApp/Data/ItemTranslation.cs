using zms9110750.WarframeMarketApi.Models.Items;

namespace WarframeMarketApp.Data;

/// <summary>
/// 翻译记录。继承 <see cref="LanguagePake"/>，增加 ItemId 和 Language 字段。
/// 用于映射 ItemTranslations 表和 SqlQuery 查询。
/// </summary>
/// <param name="ItemId">物品 ID</param>
/// <param name="Language">语言代码（en/zh-hans 等）</param>
/// <param name="Name">物品名称</param>
/// <param name="Description">描述</param>
/// <param name="WikiLink">Wiki 链接</param>
/// <param name="Icon">图标路径</param>
/// <param name="Thumb">缩略图路径</param>
/// <param name="SubIcon">子图标路径</param>
public record ItemTranslation(
	string ItemId,
	string Language,
	string Name,
	string? Description,
	string? WikiLink,
	string Icon,
	string Thumb,
	string? SubIcon
) : LanguagePake(Name, Description, WikiLink, Icon, Thumb, SubIcon);
