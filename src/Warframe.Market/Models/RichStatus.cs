namespace zms9110750.WarframeMarketApi.Models;

/// <summary>
/// 用户详细状态（用于状态端点）
/// </summary>
/// <param name="Status">状态字符串</param>
/// <param name="StatusUntil">状态过期时间（ISO 8601）</param>
/// <param name="StatusSetAt">状态设置时间（ISO 8601）</param>
/// <param name="Activity">当前活动</param>
public record RichStatus(
    string? Status,
    string? StatusUntil,
    string? StatusSetAt,
    Users.Activity? Activity
);
