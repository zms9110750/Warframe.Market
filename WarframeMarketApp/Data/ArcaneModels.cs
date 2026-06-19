namespace WarframeMarketApp.Data;

/// <summary>
/// 赋能包 YAML 配置模型
/// </summary>
public class ArcanePackConfig
{
	public string Name { get; set; } = "";
	public ArcaneQualityGroup[] Items { get; set; } = Array.Empty<ArcaneQualityGroup>();

	/// <summary>获取某个物品在这个包里的出现概率</summary>
	public double GetProbability(string itemName)
	{
		foreach (var q in Items)
		{
			foreach (var item in q.Items)
			{
				if (item == itemName)
					return q.Quality / q.Items.Length;
			}
		}
		return 0;
	}
}

public class ArcaneQualityGroup
{
	public string Subtypes { get; set; } = ""; // Common / Uncommon / Rare / Legendary
	public double Quality { get; set; }
	public string[] Items { get; set; } = Array.Empty<string>();
}
