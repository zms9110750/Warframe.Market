using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WarframeMarketApp.Data;

namespace WarframeMarketApp.Services;

/// <summary>
/// 通用本地缓存服务。Value 存 JSON。
/// CachedAt 由 SQLite 自动填。DaysOld > 0 时后台刷新，≥ 2 时失效。
/// </summary>
public class LocalCacheService
{
	private readonly WfmDbContext _db;
	private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);
	private readonly HashSet<string> _refreshing = new();

	public LocalCacheService(WfmDbContext db) => _db = db;

	/// <summary>启动时清理跨 2 天以上的旧缓存</summary>
	public async Task CleanupAsync()
	{
		await _db.Database.ExecuteSqlRawAsync(
			"DELETE FROM Cache WHERE (julianday('now') - julianday(CachedAt)) >= 2");
	}

	/// <summary>读缓存。未命中或跨 ≥ 2 天返回 null</summary>
	public async Task<T?> GetAsync<T>(string key) where T : class
	{
		var entry = await _db.Cache.FindAsync(key);
		if (entry == null) return null;

		if (entry.DaysOld >= 2)
		{
			_db.Cache.Remove(entry);
			await _db.SaveChangesAsync();
			return null;
		}

		if (entry.DaysOld > 0)
			_ = TryRefreshAsync(key);

		return JsonSerializer.Deserialize<T>(entry.Value, JsonOpts);
	}

	/// <summary>写入缓存</summary>
	public async Task SetAsync<T>(string key, T value)
	{
		var json = JsonSerializer.Serialize(value, JsonOpts);
		var existing = await _db.Cache.FindAsync(key);

		if (existing != null)
		{
			existing.Value = json;
			existing.CachedAt = DateTime.UtcNow;
		}
		else
		{
			// 新行：SQLite 自动填 CachedAt
			_db.Cache.Add(new CacheEntry { Key = key, Value = json });
		}
		await _db.SaveChangesAsync();
	}

	/// <summary>删除缓存</summary>
	public async Task RemoveAsync(string key)
	{
		var entry = await _db.Cache.FindAsync(key);
		if (entry != null)
		{
			_db.Cache.Remove(entry);
			await _db.SaveChangesAsync();
		}
	}

	private async Task TryRefreshAsync(string key)
	{
		lock (_refreshing)
		{
			if (!_refreshing.Add(key)) return;
		}
		try
		{
			await RefreshAsync(key);
		}
		finally
		{
			lock (_refreshing) { _refreshing.Remove(key); }
		}
	}

	protected virtual Task RefreshAsync(string key) => Task.CompletedTask;
}
