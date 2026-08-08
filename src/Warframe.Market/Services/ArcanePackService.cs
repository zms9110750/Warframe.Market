using zms9110750.WarframeMarketApi.Models.Arcane;
using zms9110750.WarframeMarketApi.Models.Statistics;

namespace zms9110750.WarframeMarketApi.Services;

/// <summary>
/// 赋能包期望值计算实现：Σ(材料价 × 出货率 × 有效量)，购买量&gt;0 时流动性封顶
/// </summary>
public class ArcanePackService : IArcanePackService
{
    private readonly IItemSearchService _items;

    // 一组小小黑(420荧尘) × 6组/包 / 200(每包价格) × 3(每包开3个赋能)
    public const double PackGainRate = 420.0 * 6 / 200 * 3; // = 37.8

    public ArcanePackService(IItemSearchService items)
    {
        _items = items;
    }

    public async Task<double> GetReferencePriceAsync(ArcanePackConfig pack, int purchase = 0)
    {
        double total = 0;
        int reqCount = 0;
        foreach (var q in pack.Items)
        {
            foreach (var itemName in q.Items)
            {
                var item = await _items.FindByKeyAsync(itemName);
                if (item == null)
                {
                    continue;
                }

                var slug = item.Slug;

                reqCount++;
                if (reqCount % 3 == 0)
                {
                    await Task.Delay(500);
                }

                var stat = await _items.GetStatisticAsync(slug);
                if (stat == null)
                {
                    continue;
                }

                var maxPrice = _items.GetMaxReferencePrice(stat);
                if (maxPrice == null)
                {
                    continue;
                }

                var firstRanked = stat.Payload?.StatisticsClosed?.Day90?.FirstOrDefault(e => e.ModRank > 0);
                var rank = firstRanked?.ModRank ?? 0;
                var syn = rank is > 0 and <= 5 ? StatisticPrice.SyntheticConsumption[rank] : 1;
                var materialPrice = maxPrice.Value / syn;

                var prob = pack.GetProbability(itemName);
                var effectiveVolume = prob * PackGainRate;

                if (purchase > 0 && stat.Payload?.StatisticsClosed?.Day90 != null)
                {
                    var dailyVolume = GetDailyVolume(stat);
                    effectiveVolume = Math.Min(effectiveVolume, dailyVolume / purchase);
                }

                total += materialPrice * effectiveVolume;
            }
        }
        return total;
    }

    public double GetDailyVolume(Statistic? stat)
    {
        if (stat?.Payload?.StatisticsClosed?.Day90 == null)
        {
            return 0;
        }

        return stat.Payload.StatisticsClosed.Day90
            .Sum(e => e.Volume * (e.ModRank is > 0 and <= 5 ? StatisticPrice.SyntheticConsumption[e.ModRank.Value] : 1)) / 90.0;
    }
}
