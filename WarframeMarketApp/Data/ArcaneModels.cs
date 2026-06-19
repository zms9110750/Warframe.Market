namespace WarframeMarketApp.Data;

/// <summary>
/// 赋能包 YAML 配置模型
/// </summary>
public class ArcanePackConfig
{
	public string Name { get; set; } = "";
	public ArcaneQualityGroup[] Items { get; set; } = Array.Empty<ArcaneQualityGroup>();
}

public class ArcaneQualityGroup
{
	public string Subtypes { get; set; } = ""; // Common / Uncommon / Rare / Legendary
	public double Quality { get; set; }
	public string[] Items { get; set; } = Array.Empty<string>();
}
