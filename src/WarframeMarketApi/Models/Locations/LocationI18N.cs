namespace zms9110750.WarframeMarketApi.Models.Locations;

/// <summary>
/// 位置节点的本地化信息
/// </summary>
/// <param name="NodeName">节点显示名称</param>
/// <param name="SystemName">星系名称</param>
/// <param name="Icon">图标路径</param>
/// <param name="Thumb">缩略图路径</param>
public record LocationI18N(
	string NodeName,
	string SystemName,
	string Icon,
	string Thumb
);
