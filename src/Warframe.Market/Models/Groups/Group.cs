namespace zms9110750.WarframeMarketApi.Models.Groups;

/// <summary>
/// 用户自定义订单/合同分组
/// </summary>
/// <param name="Id">唯一标识符</param>
/// <param name="UserId">所属用户 ID</param>
/// <param name="Kind">分组类型（orders / contracts）</param>
/// <param name="Name">显示名称</param>
public record Group(
    string Id,
    string UserId,
    string Kind,
    string Name
);
