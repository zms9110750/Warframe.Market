namespace zms9110750.WarframeMarketApi.Models.Liches;

/// <summary>
/// 巫妖武器信息
/// </summary>
/// <param name="Id">唯一标识符</param>
/// <param name="Slug">URL 友好名称</param>
/// <param name="GameRef">游戏内路径引用</param>
/// <param name="ReqMasteryRank">所需精通等级</param>
/// <param name="I18n">多语言本地化信息</param>
public record LichWeapon(
    string Id,
    string Slug,
    string GameRef,
    int ReqMasteryRank,
    Dictionary<Items.Language, LichWeaponI18N> I18n
)
{
    public static implicit operator string(LichWeapon item)
    {
        return item.Slug;
    }
}
