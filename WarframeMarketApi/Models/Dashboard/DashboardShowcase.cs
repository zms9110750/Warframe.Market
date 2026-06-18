namespace zms9110750.WarframeMarketApi.Models.Dashboard;

/// <summary>
/// 移动端主页展示面板
/// </summary>
/// <param name="I18n">多语言本地化文本（标题和描述）</param>
/// <param name="Items">精选物品列表</param>
public record DashboardShowcase(
	Dictionary<Items.Language, DashboardShowcaseI18N>? I18n,
	DashboardShowcaseItem[] Items
);
