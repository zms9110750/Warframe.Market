namespace zms9110750.WarframeMarketApi.Models.Dashboard;

/// <summary>
/// 展示面板中的物品项
/// </summary>
/// <param name="Item">物品 slug / key</param>
/// <param name="Background">背景图路径</param>
/// <param name="BigCard">是否以大卡片渲染</param>
/// <param name="Label">标签文本</param>
/// <param name="LabelPosition">标签位置</param>
public record DashboardShowcaseItem(
	string Item,
	string Background,
	bool BigCard,
	string? Label,
	string? LabelPosition
);
