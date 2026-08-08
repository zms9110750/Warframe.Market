namespace zms9110750.WarframeMarketApi.Models.Arcane;

/// <summary>赋能包配置（赋能包配置.yaml）</summary>
public class ArcanePackConfig
{
    public string Name { get; set; } = "";
    public ArcaneQualityGroup[] Items { get; set; } = [];

    /// <summary>出货率 = 品质权重 ÷ 同组物品数</summary>
    public double GetProbability(string itemName)
    {
        var group = Items.FirstOrDefault(q => q.Items.Contains(itemName));
        return group == null || group.Items.Length == 0 ? 0 : group.Quality / group.Items.Length;
    }
}

/// <summary>赋能包内的品质组</summary>
public class ArcaneQualityGroup
{
    /// <summary>品质标签（Common / Uncommon / Rare / Legendary，yaml 为标量）</summary>
    public string Subtypes { get; set; } = "";
    public double Quality { get; set; }
    public string[] Items { get; set; } = [];
}
