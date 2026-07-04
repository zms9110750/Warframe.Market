using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace WarframeMarketQuery.Model;

/// <summary>
/// V2 API响应模型
/// </summary>
/// <param name="ApiVersion">API版本</param>
/// <param name="Data">数据</param>
/// <param name="Error">错误</param>
public record Response<T>(
	string ApiVersion,
	T Data,
	string? Error);
