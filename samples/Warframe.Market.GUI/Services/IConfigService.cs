using zms9110750.WarframeMarketApi.Models.Arcane;
using zms9110750.Warframe.Market.GUI.Data;

namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>配置读写（用户默认配置 config.yaml / 赋能包配置 / ui-config）</summary>
public interface IConfigService
{
    /// <summary>读用户默认配置（语言/平台/跨平台/已下载语言包）；不存在则创建默认</summary>
    AppConfig LoadAppConfig();

    /// <summary>保存用户默认配置</summary>
    void SaveAppConfig(AppConfig config);

    /// <summary>读赋能包配置；不存在则写入默认，损坏返回空数组</summary>
    ArcanePackConfig[] LoadArcaneConfig();

    /// <summary>读 ui-config（快捷输入/固定按钮）</summary>
    UIConfig LoadUIConfig();
}
