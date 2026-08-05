using System.Globalization;

namespace zms9110750.WarframeMarketApi.Models.Orders;

/// <summary>
/// 已全部或部分关闭的订单的交易记录
/// </summary>
/// <param name="Id">交易唯一标识符</param>
/// <param name="Type">交易类型</param>
/// <param name="OriginId">原始订单标识符</param>
/// <param name="Platinum">铂金数量</param>
/// <param name="Quantity">物品数量</param>
/// <param name="CreatedAt">创建时间（ISO 8601）</param>
/// <param name="UpdatedAt">最后修改时间（ISO 8601）</param>
/// <param name="Item">交易涉及的物品明细</param>
/// <param name="User">交易涉及的用户</param>
public record Transaction(
    string Id,
    string Type,
    string OriginId,
    double Platinum,
    int Quantity,
    string CreatedAt,
    string UpdatedAt,
    TxItem Item,
    Users.UserShort User
)
{
    /// <summary>创建时间（本地时间）</summary>
    public DateTime CreatedAtLocal => DateTime.Parse(CreatedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToLocalTime();
    /// <summary>最后修改时间（本地时间）</summary>
    public DateTime UpdatedAtLocal => DateTime.Parse(UpdatedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToLocalTime();
}
