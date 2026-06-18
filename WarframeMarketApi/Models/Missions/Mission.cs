namespace zms9110750.WarframeMarketApi.Models.Missions;

/// <summary>
/// 任务信息
/// </summary>
/// <param name="Id">唯一标识符</param>
/// <param name="Slug">URL 友好名称</param>
/// <param name="GameRef">游戏内路径引用</param>
/// <param name="I18n">多语言本地化信息</param>
public record Mission(
	string Id,
	string Slug,
	string GameRef,
	Dictionary<Items.Language, MissionI18N> I18n
);
