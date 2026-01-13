using Microsoft.Extensions.Caching.Hybrid;
using System.Runtime.CompilerServices;
using System.Text.Json;
using WarframeMarketQuery.API;
using WarframeMarketQuery.Arcane;
using WarframeMarketQuery.Extension;
using WarframeMarketQuery.Model;
using WarframeMarketQuery.Model.Items;
using WarframeMarketQuery.Model.Statistics;

namespace WarframeMarketQuery.Extension;

public static class ModelExtension
{
    extension(HybridCache cache)
    {
        public ValueTask<T?> GetOrDefaultAsync<T>(string key, T? defaultValue = default, CancellationToken cancellationToken = default)
        {
            return cache.GetOrCreateAsync(key, _ => ValueTask.FromResult(defaultValue), GetCacheEntryOptions, cancellationToken: cancellationToken);
        }
    }
    static HybridCacheEntryOptions GetCacheEntryOptions { get; } = new HybridCacheEntryOptions
    {
        Flags = HybridCacheEntryFlags.DisableLocalCacheWrite | HybridCacheEntryFlags.DisableDistributedCacheWrite
    };


    /// <summary>
    /// 赋能合成消耗
    /// </summary>
    public static IReadOnlyList<int> SyntheticConsumption { get; } = [1, 3, 6, 10, 15, 21];

    /// <summary>
    /// 默认权重
    /// </summary>
    static IReadOnlyList<double> DefaultWeight { get; } = [40, 25, 15, 5, 5, 5, 5];

    /// <summary>
    /// 获取参考价格
    /// </summary>
    /// <param name="statistic">统计数据</param> 
    /// <param name="filter">过滤器</param>
    public static double GetReferencePrice(this Statistic statistic, Func<Entry, bool>? filter = null)
    {
        // 获取过去 90 天的统计数据，并根据 filter 过滤，按时间降序排序
        var filteredEntries = statistic.Payload.StatisticsClosed.Day90
            .Where(filter ?? (static s => s is
            {
                ModRank: not > 0,
                AmberStars: not > 0,
                Subtype: not (ItemSubtypes.Crafted or ItemSubtypes.Radiant)
            }))
            .OrderByDescending(x => x.Datetime)
            .Zip(DefaultWeight)
            .ToArray(); // 将数据与权重配对 

        // 初始化总权重和加权总和
        var totalWeight = 0.0;
        var weightedSum = 0.0;

        // 遍历每个数据项，计算加权总和和总权重
        foreach (var (entry, weightValue) in filteredEntries)
        {
            var weightedVolume = entry.Volume * weightValue;
            totalWeight += weightedVolume;
            weightedSum += weightedVolume * entry.Median;
        }

        // 返回加权平均价
        return weightedSum / totalWeight;
    }

    /// <summary>
    /// 获取参考价格
    /// </summary>
    /// <param name="statistic">统计数据</param>
    /// <param name="subtype">筛选为指定的子类型</param>
    /// <returns></returns>
    public static double GetReferencePrice(this Statistic statistic, ItemSubtypes subtype)
    {
        return GetReferencePrice(statistic, e => e.Subtype == subtype);
    }

    /// <summary>
    /// 获取满级参考价格
    /// </summary>
    /// <param name="statistic">统计数据</param>
    /// <returns></returns>
    public static double GetMaxReferencePrice(this Statistic statistic)
    {
        return GetReferencePrice(statistic, entry => entry is
        {
            ModRank: not 0,
            AmberStars: not 0,
            Subtype: ItemSubtypes.Crafted or ItemSubtypes.Radiant or ItemSubtypes.Magnificent or ItemSubtypes.Large or null
        });
    }

    /// <summary>
    /// 获取赋能用满级价格推算一级的价格
    /// </summary>
    /// <param name="statistic">统计数据</param>
    /// <returns></returns>
    public static double GetMaterialBasedReferencePrice(this Statistic statistic)
    {
        return GetMaxReferencePrice(statistic) / (statistic.Payload.StatisticsClosed.Day90.FirstOrDefault(s => s.ModRank != 0)?.ModRank switch
        {
            > 0 and <= 5 and int rank => SyntheticConsumption[rank],
            _ => 1
        });
    }
    /// <summary>
    /// 一组小小黑可以买的赋能包能开出来的赋能数量
    /// </summary>
    public const double PackGainRate = 420.0 * 6 / 200 * 3;
    /// <summary>
    /// 获取一组小小黑赋能全部分解为荧尘，然后用全部的荧尘买这个赋能包后的，开出的赋能的期望价格
    /// </summary>
    /// <param name="package">赋能包</param> 
    /// <param name="client">wm访问器</param>
    /// <param name="purchase">每天购入的小小黑组数。如果开出的赋能比市场流通的还多，按照市场流通数量算期望</param>
    /// <returns></returns>
    public static async Task<double> GetReferencePriceAsync(this ArcanePack package, WarframeMarketApi client, int purchase = 0, CancellationToken token = default)
    {
        var result = package
            .SelectMany(s => s)
            .Select(async item =>
            {
                var statistics = (await client.GetStatisticByIndexAsync(item, token));
                //GetProbability 获取这个道具在这个包里开出来的概率。
                //PackGainRate 一组小小黑可以买的赋能包能开出来的赋能数量
                double effectiveVolume = package.GetProbability(item) * PackGainRate;
                if (purchase != 0)
                {
                    //statistics.Payload.StatisticsClosed.Day90 90天内结算的订单数据。
                    //entry => entry.Volume * SyntheticConsumption[s.ModRank ?? 0]) 这个等级对应的合成消耗数量（把满级的转为一级，一级的不变）
                    effectiveVolume = Math.Min(effectiveVolume, statistics.Payload.StatisticsClosed.Day90.Sum(entry => entry.Volume * SyntheticConsumption[entry.ModRank ?? 0]) / 90 / purchase);
                }
                return statistics.GetMaterialBasedReferencePrice() * effectiveVolume;
            });
        return (await Task.WhenAll(result)).Sum();
    }

    public static async IAsyncEnumerable<IList<T>> Buffer<T>(this IAsyncEnumerable<T> source, TimeSpan timeSpan, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var enumerator = source.GetAsyncEnumerator();
        List<T> buffer = new();
        Task delayTask = Task.CompletedTask;
        while (true)
        {
            ValueTask<bool> next = enumerator.MoveNextAsync();
            if (next.IsCompleted)
            {
                if (next.Result)
                {
                    buffer.Add(enumerator.Current);
                    continue;

                }
                else
                {
                    yield return buffer;
                    yield break;
                }
            }
            if (delayTask.IsCompleted)
            {
                if (await next)
                {
                    buffer.Add(enumerator.Current);
                    delayTask = Task.Delay(timeSpan, cancellationToken);
                }
                else
                {
                    yield return buffer;
                    yield break;
                }
            }
            else
            {
                var nextTask = next.AsTask();
                if (await Task.WhenAny(nextTask, delayTask) == delayTask)
                {
                    yield return buffer;
                    buffer.Clear();
                    if (await nextTask)
                    {
                        buffer.Add(enumerator.Current);
                        delayTask = Task.Delay(timeSpan, cancellationToken);
                    }
                    else
                    {
                        yield break;
                    }
                }
                else if (await nextTask)
                {
                    buffer.Add(enumerator.Current);
                }
                else
                {
                    yield return buffer;
                    yield break;
                }
            }
        }
    }
}

