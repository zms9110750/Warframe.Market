namespace zms9110750.WarframeMarketApi.Models.Liches;

/// <summary>
/// 巫妖怪癖的本地化信息
/// </summary>
/// <param name="Name">显示名称</param>
/// <param name="Description">怪癖描述</param>
/// <param name="Icon">图标路径</param>
/// <param name="Thumb">缩略图路径</param>
public record LichQuirkI18N(
    string Name,
    string? Description,
    string Icon,
    string Thumb
);
