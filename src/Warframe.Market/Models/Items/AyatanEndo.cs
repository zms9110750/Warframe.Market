namespace zms9110750.WarframeMarketApi.Models.Items;

/// <summary>
/// 阿耶檀识塑像：把镶嵌的星星归一化为可兑换的内融核心（豆子）数量。
/// 公式（Wiki）：内融核心 = (B + 50×青蓝 + 100×琥珀) × (1 + M×(青蓝+琥珀) ÷ S)
/// B=基础价值（详情 BaseEndo）、C=青蓝星、A=琥珀星、S=插槽数（MaxAmber+MaxCyan）、M=塑像系数。
/// </summary>
public static class AyatanEndo
{
    /// <summary>
    /// M 系数：Wiki 简略写为 Anasa=0.5、Zambuka=3.0、其余=2.0；
    /// 实测表格数据反推 B=450 的 4 个塑像（Zambuka/Chattraka/Hemakara/Kitha）全部为 3.0——按 B 兜底。
    /// </summary>
    public static double GetMultiplier(int baseEndo)
    {
        return baseEndo switch {
            2000 => 0.5,  // Anasa
            450 => 3.0,   // Zambuka / Chattraka / Hemakara / Kitha
            _ => 2.0,
        };
    }

    /// <summary>星星归一化为内融核心（四舍五入）</summary>
    public static int Compute(int baseEndo, int amberStars, int cyanStars, int slots)
    {
        var m = GetMultiplier(baseEndo);
        return (int)Math.Round(
            (baseEndo + 50.0 * cyanStars + 100.0 * amberStars)
            * (1 + m * (amberStars + cyanStars) / (double)slots),
            MidpointRounding.AwayFromZero); // 表格值如 2362.5→2363（游戏向上取整）
    }

    /// <summary>
    /// 塑像目录：slug → (基础豆子, 最大琥珀星, 最大青蓝星)。
    /// 列表响应（ItemShort）不含 baseEndo/maxStars（详情才有）——GUI 按 slug 查表算豆子；未知塑像返回 null。
    /// </summary>
    private static readonly Dictionary<string, (int BaseEndo, int MaxAmber, int MaxCyan)> Catalog = new(StringComparer.OrdinalIgnoreCase) {
        ["ayatan_anasa_sculpture"] = (2000, 2, 2),
        ["ayatan_ayr_sculpture"] = (325, 0, 3),
        ["ayatan_chattraka_sculpture"] = (450, 1, 2),
        ["ayatan_orta_sculpture"] = (650, 1, 3),
        ["ayatan_piv_sculpture"] = (375, 1, 2),
        ["ayatan_sah_sculpture"] = (300, 1, 2),
        ["ayatan_valana_sculpture"] = (325, 1, 2),
        ["ayatan_vaya_sculpture"] = (400, 1, 2),
        ["ayatan_zambuka_sculpture"] = (450, 1, 2),
        ["ayatan_kitha_sculpture"] = (450, 1, 4),
        ["ayatan_hemakara_sculpture"] = (450, 1, 2),
    };

    /// <summary>
    /// 豆子滑块步进（按各塑像豆子值的相邻最小差）：57 → 50；75~85 → 75；其余已整数 → 原值。
    /// </summary>
    private static readonly Dictionary<string, int> Steps = new(StringComparer.OrdinalIgnoreCase) {
        ["ayatan_anasa_sculpture"] = 50,    // min 差 57
        ["ayatan_ayr_sculpture"] = 300,     // 300
        ["ayatan_chattraka_sculpture"] = 100, // 100
        ["ayatan_orta_sculpture"] = 75,     // 75
        ["ayatan_piv_sculpture"] = 75,      // 84
        ["ayatan_sah_sculpture"] = 75,      // 84
        ["ayatan_valana_sculpture"] = 75,   // 83
        ["ayatan_vaya_sculpture"] = 75,     // 83
        ["ayatan_zambuka_sculpture"] = 100, // 100
        ["ayatan_kitha_sculpture"] = 75,    // 80
        ["ayatan_hemakara_sculpture"] = 100, // 100
    };

    /// <summary>豆子滑块步进（未知塑像返回 1）</summary>
    public static int GetStep(string slug)
    {
        return Steps.TryGetValue(slug, out var s) ? s : 1;
    }

    /// <summary>从塑像目录按 slug 算豆子（列表响应无详情字段时）；未知塑像返回 null</summary>
    public static int? FromSlug(string slug, int amberStars, int cyanStars)
    {
        return Catalog.TryGetValue(slug, out var d)
            ? Compute(d.BaseEndo, amberStars, cyanStars, d.MaxAmber + d.MaxCyan)
            : null;
    }

    /// <summary>豆子范围（无星 ~ 满星）；未知塑像返回 null——用于安魂滑块的最小/最大值</summary>
    public static (int Min, int Max)? RangeFromSlug(string slug)
    {
        if (!Catalog.TryGetValue(slug, out var d))
        {
            return null;
        }

        var slots = d.MaxAmber + d.MaxCyan;
        return (Compute(d.BaseEndo, 0, 0, slots), Compute(d.BaseEndo, d.MaxAmber, d.MaxCyan, slots));
    }

    /// <summary>从物品详情字段计算（B=BaseEndo、S=MaxAmberStars+MaxCyanStars）；非安魂返回 null</summary>
    public static int? FromItem(Item item, int amberStars, int cyanStars)
    {
        var slots = (item.MaxAmberStars ?? 0) + (item.MaxCyanStars ?? 0);
        if (slots <= 0 || item.BaseEndo is not > 0)
        {
            return null;
        }

        return Compute(item.BaseEndo!.Value, amberStars, cyanStars, slots);
    }
}
