using zms9110750.WarframeMarketApi.Models.Arcane;
using zms9110750.WarframeMarketApi.Models.Statistics;

namespace zms9110750.WarframeMarketApi.Services;

/// <summary>
/// 赋能包期望值计算实现：Σ(材料价 × 出货率 × 有效量)，购买量&gt;0 时流动性封顶
/// </summary>
public class ArcanePackService : IArcanePackService
{
    private readonly IItemSearchService _items;

    // 本次页面生命周期内用过的统计 slug（页面关闭时 SetStatisticsPriority 批量降级）
    private readonly HashSet<string> _usedSlugs = new();

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
                    Log.Warning("赋能包物品未命中索引: {Pack}/{Item}", pack.Name, itemName);
                    continue;
                }

                var slug = item.Slug;
                _usedSlugs.Add(slug); // 记录：页面关闭时统一降级优先级

                reqCount++;
                if (reqCount % 3 == 0)
                {
                    await Task.Delay(500);
                }

                var stat = await _items.GetStatisticAsync(slug);
                if (stat == null)
                {
                    Log.Warning("赋能包物品统计为空: {Pack}/{Item}", pack.Name, itemName);
                    continue;
                }

                var maxPrice = _items.GetMaxReferencePrice(stat);
                if (maxPrice == null)
                {
                    Log.Warning("赋能包物品无最大参考价: {Pack}/{Item}", pack.Name, itemName);
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

    /// <summary>赋能包页面关闭时：把本次用过的统计条目降级（路由离开 → Normal 可逐出）</summary>
    public void SetStatisticsPriority(Microsoft.Extensions.Caching.Memory.CacheItemPriority priority)
    {
        foreach (var slug in _usedSlugs)
        {
            _items.SetStatisticPriority(slug, priority);
        }
        _usedSlugs.Clear();
    }
}
