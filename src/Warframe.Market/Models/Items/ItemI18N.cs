namespace zms9110750.WarframeMarketApi.Models.Items;

/// <summary>
/// 物品的本地化名称、描述和图标等信息
/// </summary>
/// <param name="Name">显示名称</param>
/// <param name="Description">物品描述</param>
/// <param name="WikiLink">Wiki 页面链接</param>
/// <param name="Icon">图标路径</param>
/// <param name="Thumb">缩略图路径</param>
/// <param name="SubIcon">子图标路径</param>
public record ItemI18N(
    string Name,
    string? Description,
    string? WikiLink,
    string Icon,
    string Thumb,
    string? SubIcon
);
