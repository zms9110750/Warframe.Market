namespace zms9110750.WarframeMarketApi.Models.Npcs;

/// <summary>
/// NPC 信息
/// </summary>
/// <param name="Id">唯一标识符</param>
/// <param name="Slug">URL 友好名称</param>
/// <param name="GameRef">游戏内路径引用</param>
/// <param name="I18n">多语言本地化信息</param>
public record Npc(
    string Id,
    string Slug,
    string GameRef,
    Dictionary<Items.Language, NpcI18N> I18n
)
{
    public static implicit operator string(Npc item)
    {
        return item.Slug;
    }
}
