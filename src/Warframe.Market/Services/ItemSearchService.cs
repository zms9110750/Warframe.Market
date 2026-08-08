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
    private readonly SemaphoreSlim _lock = new(1, 1);

    private Trie? _trie;
    private Dictionary<string, ItemShort>? _byId;
    private Dictionary<string, ItemShort>? _bySlug;
    private Dictionary<string, ItemShort>? _byName;
    private bool _loaded;

    public ItemSearchService(WarframeMarketClient wfm)
    {
        _wfm = wfm;
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
        // 全局统计并发限制（3）：避免赋能包多任务并行触发请求风暴挤爆限流队列
        await StatThrottle.WaitAsync(ct);
        try
        {
            return await _wfm.GetStatisticsAsync(slug, ct);
        }
        catch { return null; }
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
