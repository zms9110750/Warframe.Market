using Xunit;
using zms9110750.Warframe.Market.GUI.Services;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>TabSessionState（物品/用户搜索共用的 tab 生命周期状态）纯单测</summary>
public class TabSessionStateTests
{
    [Fact]
    public void Add_appends_tab_and_activates_it()
    {
        var s = new TabSessionState<string>();
        s.Add("欢迎");
        s.Add("甲");
        s.Add("乙");

        Assert.Equal(["欢迎", "甲", "乙"], s.Tabs);
        Assert.Equal(2, s.ActiveIndex);
    }

    [Fact]
    public void Add_with_pinned_records_pin()
    {
        var s = new TabSessionState<string>();
        s.Add("欢迎");
        s.Add("甲", pinned: true);

        Assert.Contains("甲", s.Pinned);
        Assert.DoesNotContain("欢迎", s.Pinned);
    }

    [Fact]
    public void RemoveAt_rejects_welcome_tab()
    {
        var s = new TabSessionState<string>();
        s.Add("欢迎");
        s.Add("甲");

        Assert.Null(s.RemoveAt(0)); // 欢迎 tab 不可关
        Assert.Equal(2, s.Tabs.Count);
    }

    [Fact]
    public void RemoveAt_rejects_out_of_range()
    {
        var s = new TabSessionState<string>();
        s.Add("欢迎");
        s.Add("甲");

        Assert.Null(s.RemoveAt(5));
        Assert.Equal(2, s.Tabs.Count);
    }

    [Fact]
    public void RemoveAt_cancels_cts_and_clamps_active_index()
    {
        var s = new TabSessionState<string>();
        s.Add("欢迎");
        s.Add("甲");
        s.Add("乙");
        var cts = s.GetOrCreateCts("乙");
        s.ActiveIndex = 2;

        var removed = s.RemoveAt(2);

        Assert.Equal("乙", removed);
        Assert.True(cts.IsCancellationRequested); // 关闭时取消未完成任务
        Assert.False(s.Cts.ContainsKey("乙"));
        Assert.Equal(["欢迎", "甲"], s.Tabs);
        Assert.Equal(1, s.ActiveIndex);
    }

    [Fact]
    public void TogglePin_toggles_both_ways()
    {
        var s = new TabSessionState<string>();
        s.Add("欢迎");

        s.TogglePin("甲");
        Assert.Contains("甲", s.Pinned);

        s.TogglePin("甲");
        Assert.DoesNotContain("甲", s.Pinned);
    }

    [Fact]
    public void GetOrCreateCts_reuses_existing_token()
    {
        var s = new TabSessionState<string>();
        var a = s.GetOrCreateCts("甲");
        var b = s.GetOrCreateCts("甲");

        Assert.Same(a, b);
    }

    [Fact]
    public void CancelAndRemove_cancels_and_removes_then_recreates_fresh()
    {
        var s = new TabSessionState<string>();
        var old = s.GetOrCreateCts("甲");
        s.CancelAndRemove("甲");

        Assert.True(old.IsCancellationRequested);
        Assert.False(s.Cts.ContainsKey("甲"));

        var fresh = s.GetOrCreateCts("甲"); // 重建 token——不得复用已取消令牌
        Assert.NotSame(old, fresh);
        Assert.False(fresh.IsCancellationRequested);
    }

    [Fact]
    public void CancelAndRemove_is_idempotent()
    {
        var s = new TabSessionState<string>();
        s.GetOrCreateCts("甲");

        s.CancelAndRemove("甲");
        s.CancelAndRemove("甲"); // 第二次：key 不存在，不抛
        s.CancelAndRemove("不存在");
    }

    [Fact]
    public void CancelAll_cancels_and_clears_all()
    {
        var s = new TabSessionState<string>();
        var a = s.GetOrCreateCts("甲");
        var b = s.GetOrCreateCts("乙");

        s.CancelAll();

        Assert.True(a.IsCancellationRequested);
        Assert.True(b.IsCancellationRequested);
        Assert.Empty(s.Cts);
    }

    [Fact]
    public void Refresh_flow_uses_cancel_and_remove_semantics()
    {
        // 复现 UserSearch.RefreshUserAsync 修复前 bug：只 Cancel 不移除 → 复用已取消令牌
        var s = new TabSessionState<string>();
        var old = s.GetOrCreateCts("甲");

        // 修复前写法：old.Cancel() 后 GetOrCreateCts 返回同一个已取消 token
        old.Cancel();
        var buggy = s.GetOrCreateCts("甲");
        Assert.Same(old, buggy);
        Assert.True(buggy.IsCancellationRequested); // 已取消——请求必然立即 OCE

        // 修复后写法：CancelAndRemove 后重建
        s.CancelAndRemove("甲");
        var fresh = s.GetOrCreateCts("甲");
        Assert.NotSame(old, fresh);
        Assert.False(fresh.IsCancellationRequested);
    }
}
