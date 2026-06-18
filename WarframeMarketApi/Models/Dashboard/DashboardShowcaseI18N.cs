namespace zms9110750.WarframeMarketApi.Models.Dashboard;

/// <summary>
/// 展示面板的本地化文本
/// </summary>
/// <param name="Title">标题</param>
/// <param name="Description">描述</param>
public record DashboardShowcaseI18N(
	string Title,
	string Description
);
