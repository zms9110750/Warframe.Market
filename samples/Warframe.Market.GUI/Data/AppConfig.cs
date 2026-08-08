namespace zms9110750.Warframe.Market.GUI.Data;

/// <summary>应用配置（config.yaml）</summary>
public class AppConfig
{
    public string DefaultLanguage { get; set; } = "zh-hans";
    public string DefaultPlatform { get; set; } = "pc";
    public bool DefaultCrossplay { get; set; } = true;

    /// <summary>已下载语言包的列表（这些语言会用于缓存，物品 i18n 含多语言）</summary>
    public List<string> DownloadedLanguages { get; set; } = new();
}
