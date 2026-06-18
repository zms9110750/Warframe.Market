namespace WarframeMarketApp.Data;

/// <summary>
/// 缓存的服务端资源版本
/// </summary>
public class CachedVersion
{
	public string Id { get; set; } = "";
	public string UpdatedAt { get; set; } = "";
	public string CollectionsJson { get; set; } = "{}";
	public DateTime LastSyncedAt { get; set; }
}
