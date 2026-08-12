using Xunit;
using zms9110750.Warframe.Market.GUI.Services;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>IPersistentStorage：快捷回复/钉住搜索/钉住用户持久化（临时目录，不碰真实 AppData）</summary>
public class PersistentStorageTests
{
    private static string TempDir()
    {
        return Path.Combine(Path.GetTempPath(), $"wm-ps-{Guid.NewGuid():N}");
    }

    [Fact]
    public void Add_and_remove_quick_reply()
    {
        var dir = TempDir();
        IPersistentStorage s1 = new PersistentStorage(dir);
        s1.AddQuickReply("你好");
        s1.AddQuickReply("稍等");

        // 新实例读盘（同目录）验证持久化
        IPersistentStorage s2 = new PersistentStorage(dir);
        Assert.Contains("你好", s2.Load().QuickReplies);
        Assert.Equal(2, s2.Load().QuickReplies.Count);

        s2.RemoveQuickReply("你好");
        Assert.DoesNotContain("你好", new PersistentStorage(dir).Load().QuickReplies);
    }

    [Fact]
    public void Pin_and_unpin_search_and_user()
    {
        var dir = TempDir();
        IPersistentStorage s1 = new PersistentStorage(dir);
        s1.PinSearch("wisp");
        s1.PinUser("zms9110750");

        var data = new PersistentStorage(dir).Load();
        Assert.Contains("wisp", data.PinnedSearches);
        Assert.Contains("zms9110750", data.PinnedUsers);

        new PersistentStorage(dir).UnpinSearch("wisp");
        Assert.DoesNotContain("wisp", new PersistentStorage(dir).Load().PinnedSearches);
    }

    [Fact]
    public void Missing_file_returns_empty_data()
    {
        IPersistentStorage s = new PersistentStorage(TempDir());
        var data = s.Load();
        Assert.Empty(data.QuickReplies);
        Assert.Empty(data.PinnedSearches);
        Assert.Empty(data.PinnedUsers);
    }
}
