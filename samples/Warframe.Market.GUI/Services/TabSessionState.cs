namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>
/// 标签会话状态（物品搜索/用户搜索共用）：收敛两页重复的 tab 生命周期管理。
/// 语义与当前一致：欢迎 tab 不可关、钉住不可关、切走取消未完成任务、关闭/路由离开降级统计。
/// </summary>
public sealed class TabSessionState<TKey> where TKey : notnull
{
    /// <summary>全部标签（索引 0 = 欢迎 tab）</summary>
    public List<TKey> Tabs { get; } = new();

    /// <summary>钉住的标签（持久化，不可关闭）</summary>
    public HashSet<TKey> Pinned { get; } = new();

    /// <summary>每标签的取消令牌（切走/关闭/路由离开时取消未完成任务）</summary>
    public Dictionary<TKey, CancellationTokenSource> Cts { get; } = new();

    public int ActiveIndex { get; set; }

    /// <summary>图钉模式（开启后标签显示 pin 图标可钉住/取消）</summary>
    public bool PinMode { get; set; }

    public bool Contains(TKey key)
    {
        return Tabs.Contains(key);
    }

    public void Add(TKey key, bool pinned = false)
    {
        Tabs.Add(key);
        if (pinned)
        {
            Pinned.Add(key);
        }
        ActiveIndex = Tabs.Count - 1;
    }

    public CancellationTokenSource GetOrCreateCts(TKey key)
    {
        if (!Cts.TryGetValue(key, out var cts))
        {
            cts = new CancellationTokenSource();
            Cts[key] = cts;
        }
        return cts;
    }

    /// <summary>取消某标签的任务并移除令牌（切回时重建，避免复用已取消令牌）</summary>
    public void CancelAndRemove(TKey key)
    {
        if (Cts.TryGetValue(key, out var cts))
        {
            cts.Cancel();
            Cts.Remove(key);
        }
    }

    /// <summary>切换某标签的钉住状态</summary>
    public void TogglePin(TKey key)
    {
        if (!Pinned.Add(key))
        {
            Pinned.Remove(key);
        }
    }

    /// <summary>关闭标签（欢迎 tab 不可关；返回被关闭的标签，未关闭返回 default）</summary>
    public TKey? RemoveAt(int idx)
    {
        if (idx <= 0 || idx >= Tabs.Count)
        {
            return default; // 欢迎 tab 不可关
        }

        var key = Tabs[idx];
        CancelAndRemove(key);
        Tabs.RemoveAt(idx);
        if (ActiveIndex >= Tabs.Count)
        {
            ActiveIndex = Math.Max(0, Tabs.Count - 1);
        }
        return key;
    }

    /// <summary>路由离开：取消全部未完成任务</summary>
    public void CancelAll()
    {
        foreach (var cts in Cts.Values)
        {
            cts.Cancel();
        }
        Cts.Clear();
    }
}
