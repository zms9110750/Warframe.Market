using System.Text.Json;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Users;

namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>应用全局状态：语言/平台/跨平台直接写 API 客户端请求头，另含数据版本状态</summary>
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
    public bool IsUpdating { get; set; }

    /// <summary>快速回复可编辑开关（原顶部 AppBar 开关，移到快速回复页内）</summary>
    public bool CanWrite { get; set; }
    public bool ShowRefreshPrompt { get; set; }
    public string? StatusMessage { get; set; }

    public static string LangToStr(Language lang)
    {
        return JsonNamingPolicy.KebabCaseLower.ConvertName(lang.ToString());
    }

    public static Language StrToLang(string s)
    {
        foreach (var l in Enum.GetValues<Language>())
        {
            if (string.Equals(LangToStr(l), s, StringComparison.OrdinalIgnoreCase))
            {
                return l;
            }
        }
        return Language.En;
    }

    public static string PlatToStr(Platform p)
    {
        return JsonNamingPolicy.KebabCaseLower.ConvertName(p.ToString());
    }

    public static Platform StrToPlat(string s)
    {
        foreach (var p in Enum.GetValues<Platform>())
        {
            if (string.Equals(PlatToStr(p), s, StringComparison.OrdinalIgnoreCase))
            {
                return p;
            }
        }
        return Platform.PC;
    }
}
