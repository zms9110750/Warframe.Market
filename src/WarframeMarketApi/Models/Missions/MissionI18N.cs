namespace zms9110750.WarframeMarketApi.Models.Missions;

/// <summary>
/// 任务的本地化信息
/// </summary>
/// <param name="Name">显示名称</param>
/// <param name="Icon">图标路径</param>
/// <param name="Thumb">缩略图路径</param>
public record MissionI18N(
	string Name,
	string? Icon,
	string? Thumb
);
