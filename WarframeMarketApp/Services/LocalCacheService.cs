using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WarframeMarketApp.Data;

namespace WarframeMarketApp.Services;

/// <summary>
/// 通用本地缓存服务。
/// Value 存 JSON。过期策略：跨 1 天后台刷新，跨 2 天直接失效。
/// </summary>
public class LocalCacheService
{
	private readonly WfmDbContext _db;
	private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);
	private readonly HashSet<string> _refreshing = new();

	public LocalCacheService(WfmDbContext db) => _db = db;

	/// <summary>读缓存。未命中或跨 2 天返回 null</summary>
	public async Task<T?> GetAsync<T>(string key) where T : class
	{
		var entry = await _db.Cache.FindAsync(key);
		if (entry == null) return null;

		var age = DateTime.UtcNow - entry.CachedAt;

		if (age.TotalDays >= 2)
		{
			_db.Cache.Remove(entry);
			await _db.SaveChangesAsync();
			return null;
		}

		var data = JsonSerializer.Deserialize<T>(entry.Value, JsonOpts);

		// 跨 1 天 → 后台刷新
		if (age.TotalDays >= 1)
			_ = TryRefreshAsync(key);

		return data;
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
			_db.Cache.Add(new CacheEntry { Key = key, Value = json, CachedAt = DateTime.UtcNow });
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

	// 后台刷新：只刷新一次，防并发
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

	/// <summary>子类重写此方法实现具体刷新逻辑</summary>
	protected virtual Task RefreshAsync(string key) => Task.CompletedTask;
}
