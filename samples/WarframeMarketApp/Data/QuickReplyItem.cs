namespace WarframeMarketApp.Data;

/// <summary>
/// 快捷回复条目（持久化到 SQLite）
/// </summary>
public class QuickReplyItem
{
	public int Id { get; set; }
	public string Text { get; set; } = "";
	public int SortOrder { get; set; }
}
