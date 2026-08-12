namespace zms9110750.Warframe.Market.GUI.Data;

/// <summary>用户持久化数据（persistent.yaml）</summary>
public class PersistentData
{
    public List<string> QuickReplies { get; set; } = new();
    public List<string> PinnedSearches { get; set; } = new();
    public List<string> PinnedUsers { get; set; } = new();
}
