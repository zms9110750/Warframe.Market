namespace zms9110750.WarframeMarketApi.Models.Statistics;

/// <summary>
/// 参考价计算（V1 统计数据的加权口径）
/// </summary>
public static class StatisticPrice
{
    /// <summary>合成消耗：R0 → R1..R5 各需多少个 R0</summary>
    public static IReadOnlyList<int> SyntheticConsumption { get; } = [1, 3, 6, 10, 15, 21];
    private static readonly double[] DefaultWeight = [40, 25, 15, 5, 5, 5, 5];

    /// <summary>基础参考价：未升级条目（ModRank null 或 0）的 90 天加权中位数</summary>
    public static double? GetReferencePrice(this Statistic? stat)
    {
        if (stat?.Payload?.StatisticsClosed?.Day90 == null)
        {
            return null;
        }

        return CalcWeightedMedian(stat.Payload.StatisticsClosed.Day90,
            e => e.ModRank is null or 0);
    }

    /// <summary>满级参考价：已升级条目（ModRank &gt; 0 且子类型为成品/光辉/卓越/无暇/大型）的加权中位数</summary>
    public static double? GetMaxReferencePrice(this Statistic? stat)
    {
        if (stat?.Payload?.StatisticsClosed?.Day90 == null)
        {
            return null;
        }

        return CalcWeightedMedian(stat.Payload.StatisticsClosed.Day90,
            e => e.ModRank is > 0 &&
                 (e.Subtype is null or "crafted" or "radiant" or "magnificent" or "large"));
    }

    /// <summary>材料价：满级价 ÷ 合成消耗（用于赋能包等期望值计算）</summary>
    public static double? GetMaterialBasedReferencePrice(this Statistic? stat)
    {
        var max = GetMaxReferencePrice(stat);
        if (max == null)
        {
            return null;
        }

        var firstRanked = stat?.Payload?.StatisticsClosed?.Day90
            ?.FirstOrDefault(e => e.ModRank > 0);
        var rank = firstRanked?.ModRank;
        if (rank is > 0 and <= 5)
        {
            return max / SyntheticConsumption[rank.Value];
        }

        return max;
    }

    /// <summary>遗物"满级价"：光辉（radiant，精炼度最高档）成交加权中位数</summary>
    public static double? GetRelicRadiantPrice(this Statistic? stat)
    {
        return CalcWeightedMedianFor(stat, "radiant");
    }

    /// <summary>遗物参考价：完整（intact，精炼度最低档）成交加权中位数</summary>
    public static double? GetRelicIntactPrice(this Statistic? stat)
    {
        return CalcWeightedMedianFor(stat, "intact");
    }

    private static double? CalcWeightedMedianFor(Statistic? stat, string subtype)
    {
        if (stat?.Payload?.StatisticsClosed?.Day90 == null)
        {
            return null;
        }

        return CalcWeightedMedian(stat.Payload.StatisticsClosed.Day90, e => e.Subtype == subtype);
    }

    private static double? CalcWeightedMedian(Entry[] day90, Func<Entry, bool> filter)
    {
        var entries = day90
            .Where(filter)
            .OrderByDescending(e => e.Datetime)
            .Take(7)
            .ToArray();

        if (entries.Length == 0)
        {
            return null;
        }

        double totalWeight = 0, weightedSum = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            var w = DefaultWeight[i] * entries[i].Volume;
            totalWeight += w;
            weightedSum += w * entries[i].Median;
        }
        return totalWeight > 0 ? weightedSum / totalWeight : null;
    }
}
