namespace WarframeMarketApp.Data;

/// <summary>
/// 通用缓存行。Value 存 JSON，CachedAt 由 SQLite 自动填 UTC 时间。
/// </summary>
public class CacheEntry
{
	public string Key { get; set; } = "";
	public string Value { get; set; } = "";

	/// <summary>缓存写入时间（SQLite 自动填 UTC datetime）</summary>
	public DateTime CachedAt { get; set; }

	/// <summary>缓存距今的天数差（查询时自动计算）</summary>
	public int DaysOld => (DateTime.UtcNow.Date - CachedAt.Date).Days;
}
