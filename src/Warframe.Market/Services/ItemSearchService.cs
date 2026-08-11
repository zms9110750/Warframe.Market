using Microsoft.Extensions.Caching.Memory;
using Serilog;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Statistics;
using zms9110750.TreeCollection.Trie;

namespace zms9110750.WarframeMarketApi.Services;

/// <summary>
/// 物品搜索服务实现：内存 Trie（索引来自 items 列表，HTTP 缓存保证不重复拉取）+ 统计 + 参考价。
/// 不做 set（套装）语义：索引按单个物品 slug/id/各语言名。
/// </summary>
public class ItemSearchService : IItemSearchService
{
    private readonly WarframeMarketClient _wfm;
    private readonly Func<IEnumerable<string>>? _extraLanguagesProvider;
    private readonly SemaphoreSlim _lock = new(1, 1);

    // 统计缓存：MS IMemoryCache（可逐出，不驻留）。GetOrCreateAsync 工厂走 HTTP（Polly → FusionCache）。
    // 优先级生命周期：使用中 NeverRemove；组件关闭时 SetStatisticPriority 降级（tab 关 High / 路由走 Normal）。
    private readonly IMemoryCache? _statCache;
    private readonly HashSet<string> _active = new();
    private readonly object _activeLock = new();

    private Trie? _trie;
    private Dictionary<string, ItemShort>? _byId;
    private Dictionary<string, ItemShort>? _bySlug;
    private Dictionary<string, ItemShort>? _byName;
    private bool _loaded;

    public ItemSearchService(WarframeMarketClient wfm, Func<IEnumerable<string>>? extraLanguagesProvider = null, IMemoryCache? statCache = null)
    {
        _wfm = wfm;
        _extraLanguagesProvider = extraLanguagesProvider;
        _statCache = statCache;
    }

    public void Invalidate()
    {
        _trie = null;
        _byId = null;
        _bySlug = null;
        _byName = null;
        _loaded = false;
    }

    private async Task EnsureIndexAsync(CancellationToken ct = default)
    {
        if (_loaded)
        {
            return;
        }

        await _lock.WaitAsync();
        try
        {
            if (_loaded)
            {
                return;
            }

            var resp = await _wfm.GetItemsAsync();
            var items = resp?.Content?.Data ?? [];
            System.Diagnostics.Debug.WriteLine($"ItemsService 索引构建：{items.Length} 个物品");

            var trie = new Trie(['_', ' ', '·']);
            var byId = new Dictionary<string, ItemShort>(items.Length);
            var bySlug = new Dictionary<string, ItemShort>(items.Length);
            var byName = new Dictionary<string, ItemShort>(items.Length * 4);

            foreach (var item in items)
            {
                trie.Add(item.Slug);
                trie.Add(item.Id);
                byId[item.Id] = item;
                bySlug[item.Slug] = item;
                if (item.I18n != null)
                {
                    foreach (var (_, pake) in item.I18n)
                    {
                        if (string.IsNullOrEmpty(pake?.Name))
                        {
                            continue;
                        }

                        trie.Add(pake.Name);
                        byName.TryAdd(pake.Name, item);
                        byName.TryAdd(NormalizeName(pake.Name), item); // 归一化键（空格/·/- 等价）
                    }
                }
            }

            // 额外语言合并：设置页勾选"下载语言包"的语言 → 各请求一次 items（带该语言头）→
            // 把该语言的物品名并入索引与 I18n（显示/私信用对方语言的物品名）
            var extraLangs = _extraLanguagesProvider?.Invoke() ?? [];
            foreach (var lang in extraLangs.Distinct())
            {
                try
                {
                    var langResp = await _wfm.GetItemsAsync(ct, lang);
                    var langItems = langResp?.Content?.Data ?? [];
                    System.Diagnostics.Debug.WriteLine($"ItemsService 合并语言 {lang}: {langItems.Length} 个物品");
                    foreach (var it in langItems)
                    {
                        if (!bySlug.TryGetValue(it.Slug, out var main))
                        {
                            continue;
                        }

                        if (it.I18n == null)
                        {
                            continue;
                        }

                        foreach (var kv in it.I18n)
                        {
                            var loc = kv.Key;
                            var pake = kv.Value;
                            if (string.IsNullOrEmpty(pake?.Name))
                            {
                                continue;
                            }

                            trie.Add(pake.Name);
                            byName.TryAdd(pake.Name, main);
                            byName.TryAdd(NormalizeName(pake.Name), main);
                            main.I18n[loc] = pake; // 合并进主物品（ItemShort.I18n 现为可变）
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ItemsService 合并语言 {lang} 失败: {ex.Message}");
                }
            }

            _trie = trie;
            _byId = byId;
            _bySlug = bySlug;
            _byName = byName;
            _loaded = true;
        }
        finally { _lock.Release(); }
    }

    public async Task<ItemShort?> FindByKeyAsync(string key)
    {
        await EnsureIndexAsync();
        if (_bySlug!.TryGetValue(key, out var s))
        {
            return s;
        }

        if (_byId!.TryGetValue(key, out var i))
        {
            return i;
        }

        if (_byName!.TryGetValue(key, out var n) || _byName.TryGetValue(NormalizeName(key), out n))
        {
            return n;
        }

        try
        {
            var resp = await _wfm.GetItemAsync(key);
            return resp?.Content?.Data;
        }
        catch { return null; }
    }

    public async Task<List<ItemShort>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new();
        }

        await EnsureIndexAsync(ct);
        if (_trie == null)
        {
            return new();
        }

        var terms = query.Split('/', '\\', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (terms.Length == 0)
        {
            return new();
        }

        var resultSet = new HashSet<string>();
        var results = new List<ItemShort>();
        foreach (var term in terms)
        {
            if (term.All(c => "_ ·".Contains(c)))
            {
                continue;
            }

            var matched = _trie.Search(term);
            foreach (var m in matched)
            {
                if (resultSet.Contains(m))
                {
                    continue;
                }

                var item = await FindByKeyAsync(m);
                if (item != null && resultSet.Add(item.Id))
                {
                    results.Add(item);
                }
            }
        }
        return results;
    }

    public async Task<Statistic?> GetStatisticAsync(string slug, CancellationToken ct = default)
    {
        if (_statCache == null)
        {
            return await FetchStatisticAsync(slug, ct); // 测试场景：无缓存直接请求
        }

        // 使用中：标记 + 不逐出（组件关闭时由 SetStatisticPriority 降级）
        lock (_activeLock)
        {
            _active.Add(slug);
        }

        // MS IMemoryCache：条目可被内存压力逐出（不会永久驻留）
        return await _statCache.GetOrCreateAsync<Statistic?>(slug, async entry => {
            entry.AbsoluteExpiration = DateTimeOffset.UtcNow + TimeUntilNextUtcMidnight();
            entry.Priority = CacheItemPriority.NeverRemove; // 使用中不逐出
            return await FetchStatisticAsync(slug, ct);
        });
    }

    public void SetStatisticPriority(string slug, CacheItemPriority priority)
    {
        // 不再使用：从活动集合移除
        lock (_activeLock)
        {
            _active.Remove(slug);
        }

        // 降级缓存条目优先级（读出来重新 Set，保留到期时间）
        if (_statCache != null && _statCache.TryGetValue(slug, out Statistic? stat))
        {
            _statCache.Set(slug, stat, new MemoryCacheEntryOptions {
                AbsoluteExpiration = DateTimeOffset.UtcNow + TimeUntilNextUtcMidnight(),
                Priority = priority,
            });
        }
    }

    /// <summary>到下一个 UTC 0 的剩余时长（与 HTTP 缓存策略一致：统计每天刷新）</summary>
    private static TimeSpan TimeUntilNextUtcMidnight()
    {
        var now = DateTime.UtcNow;
        return now.Date.AddDays(1) - now;
    }

    private async Task<Statistic?> FetchStatisticAsync(string slug, CancellationToken ct)
    {
        // 全局统计并发限制（3）：避免赋能包多任务并行触发请求风暴挤爆限流队列
        await StatThrottle.WaitAsync(ct);
        try
        {
            var stat = await _wfm.GetStatisticsAsync(slug, ct);

            // 详细日志：请求结果非预期（null / 缺字段 / 无交易数据）也记录，便于定位"界面显示 -"的根因
            if (stat == null)
            {
                Log.Warning("统计请求无数据(响应 null): {Slug}", slug);
            }
            else if (stat.Payload?.StatisticsClosed == null)
            {
                Log.Warning("统计响应缺 statistics_closed: {Slug}", slug);
            }
            else if (stat.Payload.StatisticsClosed.Day90 is not { Length: > 0 })
            {
                Log.Warning("统计 90 天无交易数据(冷门物品): {Slug}", slug);
            }

            return stat;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "统计请求异常: {Slug}", slug);
            return null;
        }
        finally { StatThrottle.Release(); }
    }

    private static readonly SemaphoreSlim StatThrottle = new(3, 3);

    public double? GetReferencePrice(Statistic? stat)
    {
        return stat.GetReferencePrice();
    }

    public double? GetMaxReferencePrice(Statistic? stat)
    {
        return stat.GetMaxReferencePrice();
    }

    /// <summary>名称归一化：去掉空格 / · / - / _（WF 中文名写法差异，如"镀层 斩铁"≡"镀层·斩铁"）</summary>
    private static string NormalizeName(string s)
    {
        return new string(s.Where(c => c is not (' ' or '·' or '-' or '_')).ToArray());
    }
}
