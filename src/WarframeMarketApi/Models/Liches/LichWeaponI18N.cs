namespace zms9110750.WarframeMarketApi.Models.Liches;

/// <summary>
/// 巫妖武器的本地化信息
/// </summary>
/// <param name="Name">显示名称</param>
/// <param name="WikiLink">Wiki 页面链接</param>
/// <param name="Icon">图标路径</param>
/// <param name="Thumb">缩略图路径</param>
public record LichWeaponI18N(
	string Name,
	string? WikiLink,
	string Icon,
	string Thumb
);
