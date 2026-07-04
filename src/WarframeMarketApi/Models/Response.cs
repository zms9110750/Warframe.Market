namespace zms9110750.WarframeMarketApi.Models;

/// <summary>
/// WFM API V2 统一响应包装
/// </summary>
/// <typeparam name="T">内部数据类型</typeparam>
/// <param name="ApiVersion">API 版本号 (semVer)</param>
/// <param name="Data">响应负载，成功时包含数据</param>
/// <param name="Error">错误负载，失败时包含错误信息</param>
public record Response<T>(
	string ApiVersion,
	T Data,
	string? Error);
