namespace zms9110750.WarframeMarketApi.Models.Clients;

/// <summary>
/// OAuth 客户端信息
/// </summary>
/// <param name="Id">唯一标识符</param>
/// <param name="Slug">URL 友好名称</param>
/// <param name="Active">是否激活</param>
/// <param name="FirstParty">是否为第一方客户端</param>
/// <param name="Logo">Logo 路径</param>
/// <param name="RedirectUris">允许的重定向 URI 列表</param>
/// <param name="Scopes">授权的作用域列表</param>
/// <param name="Secret">客户端密钥</param>
/// <param name="I18n">多语言本地化客户端信息</param>
public record Client(
    string Id,
    string Slug,
    bool? Active,
    bool? FirstParty,
    string? Logo,
    string[] RedirectUris,
    string[] Scopes,
    string? Secret,
    Dictionary<Items.Language, ClientI18N>? I18n
);
