namespace zms9110750.WarframeMarketApi.Models.Users;

/// <summary>
/// 用户在线状态
/// </summary>
public enum UserStatus
{
    /// <summary>隐身（对公众暴露为离线）</summary>
    Invisible,
    /// <summary>离线</summary>
    Offline,
    /// <summary>在线</summary>
    Online,
    /// <summary>游戏中</summary>
    Ingame,
}
