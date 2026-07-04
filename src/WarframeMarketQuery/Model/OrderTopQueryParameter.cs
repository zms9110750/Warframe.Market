using WarframeMarketQuery.Model.Items;

namespace WarframeMarketQuery.Model;

/// <summary>
/// 查询在线玩家订单的查询字符串
/// </summary>
public record struct OrderTopQueryParameter
{
    /// <summary>
    /// 物品等级
    /// </summary>
    public int? Rank { get; set; }
    /// <summary>
    /// 物品等级的最大容许值。这个值存在时无视<see cref="Rank"/>
    /// </summary>
    public int? RankLt { get; set; }
    /// <summary>
    /// 剩余使用次数
    /// </summary>
    public int? Charges { get; set; }
    /// <summary>
    /// 剩余使用次数的最大容许值。这个值存在时无视<see cref="Charges"/>
    /// </summary>
    public int? ChargesLt { get; set; }
    /// <summary>
    /// 琥珀星星的数量
    /// </summary>
    public int? AmberStars { get; set; }
    /// <summary>
    /// 琥珀星星的数量的最大容许值。这个值存在时无视<see cref="AmberStars"/>
    /// </summary>
    public int? AmberStarsLt { get; set; }
    /// <summary>
    /// 青蓝星星的数量
    /// </summary>
    public int? CyanStars { get; set; }
    /// <summary>
    /// 青蓝星星的数量的最大容许值。这个值存在时无视<see cref="CyanStars"/>
    /// </summary>
    public int? CyanStarsLt { get; set; }
    /// <summary>
    /// 物品的子类型
    /// </summary>
    public ItemSubtypes? Subtype { get; set; }
}