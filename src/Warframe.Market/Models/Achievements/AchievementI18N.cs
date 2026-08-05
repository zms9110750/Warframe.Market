namespace zms9110750.WarframeMarketApi.Models.Achievements;

/// <summary>
/// 成就的本地化文本
/// </summary>
/// <param name="Name">成就名称</param>
/// <param name="Description">成就描述</param>
/// <param name="Icon">图标路径</param>
/// <param name="Thumb">缩略图路径</param>
public record AchievementI18N(
    string Name,
    string? Description,
    string Icon,
    string Thumb
);
