namespace zms9110750.WarframeMarketApi.Models.Users;

/// <summary>
/// 用户曾用名记录
/// </summary>
/// <param name="OldName">曾用名</param>
/// <param name="NewName">新名称</param>
/// <param name="ChangedAt">更改时间</param>
public record NameHistory(
	string? OldName,
	string? NewName,
	string? ChangedAt
);
