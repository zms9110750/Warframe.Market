namespace zms9110750.WarframeMarketApi.Models.Achievements;

/// <summary>
/// 成就信息
/// </summary>
/// <param name="Id">唯一标识符</param>
/// <param name="Slug">URL 友好名称</param>
/// <param name="Type">成就类型</param>
/// <param name="Secret">是否对公众隐藏</param>
/// <param name="ReputationBonus">声望奖励</param>
/// <param name="Goal">达成目标值</param>
/// <param name="I18n">多语言本地化文本</param>
/// <param name="State">当前进度状态（仅用户特定成就时存在）</param>
public record Achievement(
    string Id,
    string Slug,
    string Type,
    bool Secret,
    int ReputationBonus,
    int Goal,
    Dictionary<Items.Language, AchievementI18N> I18n,
    AchievementState? State
)
{
    public static implicit operator string(Achievement item)
    {
        return item.Slug;
    }
}
