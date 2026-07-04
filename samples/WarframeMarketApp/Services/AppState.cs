using System.Text.Json;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Users;

namespace WarframeMarketApp.Services;

/// <summary>
/// 应用全局状态
/// </summary>
public class AppState
{
	private readonly WarframeMarketClient _wfm;

	public AppState(WarframeMarketClient wfm)
	{
		_wfm = wfm;
		_wfm.Crossplay = true;
	}

	public WarframeMarketClient Client => _wfm;

	public Language Language
	{
		get => _wfm.Language;
		set => _wfm.Language = value;
	}

	public Platform Platform
	{
		get => _wfm.Platform;
		set => _wfm.Platform = value;
	}

	public bool Crossplay
	{
		get => _wfm.Crossplay;
		set => _wfm.Crossplay = value;
	}

	public string? VersionText { get; set; }
	public string? VersionUpdatedAt { get; set; }
	public bool IsUpdating { get; set; }
	public bool ShowRefreshPrompt { get; set; }
	public string? StatusMessage { get; set; }

	/// <summary>Language 转 API 字符串（kebab-case）</summary>
	public static string LangToStr(Language lang) =>
		JsonNamingPolicy.KebabCaseLower.ConvertName(lang.ToString());

	/// <summary>API 字符串转 Language</summary>
	public static Language StrToLang(string s) =>
		Enum.TryParse<Language>(s, ignoreCase: true, out var l) ? l : Language.En;

	/// <summary>Platform 转 API 字符串（kebab-case）</summary>
	public static string PlatToStr(Platform p) =>
		JsonNamingPolicy.KebabCaseLower.ConvertName(p.ToString());

	/// <summary>API 字符串转 Platform</summary>
	public static Platform StrToPlat(string s) =>
		Enum.TryParse<Platform>(s, ignoreCase: true, out var p) ? p : Platform.PC;
}
