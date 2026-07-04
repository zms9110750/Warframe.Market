namespace zms9110750.WarframeMarketApi.Models.Items;

/// <summary>
/// 物品的本地化文本信息（名称、描述、图标等）
/// </summary>
/// <param name="Name">物品名称</param>
/// <param name="Description">物品描述</param>
/// <param name="WikiLink">Wiki 链接</param>
/// <param name="Icon">图标路径</param>
/// <param name="Thumb">缩略图路径</param>
/// <param name="SubIcon">子图标路径</param>
public record LanguagePake(
	string Name,
	string? Description,
	string? WikiLink,
	string Icon,
	string Thumb,
	string? SubIcon
);
