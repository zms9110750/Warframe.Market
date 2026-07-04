namespace WarframeMarketApp.Data;

/// <summary>
/// 持久化数据存 YAML：快捷回复 + 钉住的搜索 + 钉住的用户
/// </summary>
public class PersistentData
{
	public List<string> QuickReplies { get; set; } = new();
	public List<string> PinnedSearches { get; set; } = new();
	public List<string> PinnedUsers { get; set; } = new();
}
