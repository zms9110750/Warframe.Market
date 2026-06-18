namespace zms9110750.WarframeMarketApi.Requests;

/// <summary>
/// 更新当前用户资料偏好的请求体
/// </summary>
/// <param name="About">个人简介 Markdown，最多 300 字符</param>
/// <param name="Platform">主要游戏平台</param>
/// <param name="Crossplay">是否启用跨平台交易（platform 为 switch 时不可为 true）</param>
/// <param name="Locale">UI 语言和偏好通信语言</param>
/// <param name="Theme">UI 主题（light / dark / system）</param>
/// <param name="SyncLocale">是否跨设备同步语言偏好</param>
/// <param name="SyncTheme">是否跨设备同步主题偏好</param>
public record UpdateMeRequest(
	string? About = null,
	string? Platform = null,
	bool? Crossplay = null,
	string? Locale = null,
	string? Theme = null,
	bool? SyncLocale = null,
	bool? SyncTheme = null
);
