namespace zms9110750.WarframeMarketApi.Models.Rivens;

/// <summary>
/// 裂罅属性的本地化文本
/// </summary>
/// <param name="Name">属性效果名称</param>
/// <param name="Icon">图标路径</param>
/// <param name="Thumb">缩略图路径</param>
public record RivenAttributeI18N(
	string Name,
	string Icon,
	string Thumb
);
