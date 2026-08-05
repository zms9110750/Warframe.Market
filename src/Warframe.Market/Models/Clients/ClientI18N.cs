namespace zms9110750.WarframeMarketApi.Models.Clients;

/// <summary>
/// OAuth 客户端的本地化信息
/// </summary>
/// <param name="Name">本地化客户端名称</param>
/// <param name="Description">本地化描述</param>
public record ClientI18N(
    string Name,
    string? Description
);
