namespace zms9110750.WarframeMarketApi.Models.Sisters;

/// <summary>
/// 姐妹幻纹的本地化信息
/// </summary>
/// <param name="Name">显示名称</param>
/// <param name="Icon">图标路径</param>
/// <param name="Thumb">缩略图路径</param>
public record SisterEphemeraI18N(
    string Name,
    string Icon,
    string Thumb
);
