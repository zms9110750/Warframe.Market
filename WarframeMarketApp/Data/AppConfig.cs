namespace WarframeMarketApp.Data;

/// <summary>
/// 应用配置（YAML）
/// </summary>
public class AppConfig
{
	public string DefaultLanguage { get; set; } = "zh-hans";
	public string DefaultPlatform { get; set; } = "pc";
	public bool DefaultCrossplay { get; set; } = true;
}
