using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Users;

namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>应用全局状态：语言/平台/跨平台（写 API 客户端请求头）+ 版本/编辑/链接开关等 UI 状态</summary>
public interface IAppStateService
{
    /// <summary>打开链接模式（物品名/价格渲染为可点击链接）</summary>
    bool ClickLink { get; set; }

    Language Language { get; set; }
    Platform Platform { get; set; }
    bool Crossplay { get; set; }

    /// <summary>数据版本文本（数据日期）</summary>
    string? VersionText { get; set; }
    bool IsUpdating { get; set; }

    /// <summary>快速回复可编辑开关</summary>
    bool CanWrite { get; set; }
    bool ShowRefreshPrompt { get; set; }
    string? StatusMessage { get; set; }

    string LangToStr(Language lang);
    Language StrToLang(string s);
    string PlatToStr(Platform p);
    Platform StrToPlat(string s);
}
