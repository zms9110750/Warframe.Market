namespace WarframeMarketApp.Data;

/// <summary>
/// 通用缓存行。Value 存 JSON，CachedAt 控制过期策略。
/// </summary>
public class CacheEntry
{
	public string Key { get; set; } = "";
	public string Value { get; set; } = "";
	public DateTime CachedAt { get; set; }
}
