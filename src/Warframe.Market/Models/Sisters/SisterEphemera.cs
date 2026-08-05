using zms9110750.WarframeMarketApi.Models.Items;

namespace zms9110750.WarframeMarketApi.Models.Sisters;

/// <summary>
/// 姐妹幻纹信息
/// </summary>
/// <param name="Id">唯一标识符</param>
/// <param name="Slug">URL 友好名称</param>
/// <param name="GameRef">游戏内路径引用</param>
/// <param name="Animation">动画路径</param>
/// <param name="Element">元素类型</param>
/// <param name="I18n">多语言本地化信息</param>
public record SisterEphemera(
    string Id,
    string Slug,
    string GameRef,
    string Animation,
    string Element,
    Dictionary<Items.Language, SisterEphemeraI18N> I18n
)
{
    public static implicit operator string(SisterEphemera item)
    {
        return item.Slug;
    }
}
