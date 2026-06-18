using Microsoft.EntityFrameworkCore;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Statistics;
using zms9110750.WarframeMarketApi.Models.Versions;
using WarframeMarketApp.Data;

namespace WarframeMarketApp.Services;

/// <summary>
/// 缓存服务。管理版本缓存和统计数据查询。
/// </summary>
public class CacheService
{
	private readonly WarframeMarketClient _wfm;
	private readonly WfmDbContext _db;

	public CacheService(WarframeMarketClient wfm, WfmDbContext db)
	{
		_wfm = wfm;
		_db = db;
	}

	// ─── 版本缓存 ───

	/// <summary>
	/// 从 API 获取最新版本，同时写入本地缓存
	/// </summary>
	public async Task<ServerVersion> RefreshVersionAsync(CancellationToken ct = default)
	{
		var resp = await _wfm.GetVersionsAsync(ct);
		var version = resp?.Content?.Data
			?? throw new InvalidOperationException($"版本查询失败: {resp?.StatusCode}");

		// 缓存到本地（覆盖写入）
		var existing = await _db.VersionInfos.FindAsync(new object[] { version.Id }, ct);
		if (existing != null)
			_db.VersionInfos.Remove(existing);
		_db.VersionInfos.Add(version);
		await _db.SaveChangesAsync(ct);

		return version;
	}

	/// <summary>
	/// 获取缓存的版本。没有则调用 <see cref="RefreshVersionAsync"/>
	/// </summary>
	public async Task<ServerVersion> GetCachedVersionAsync(CancellationToken ct = default)
	{
		var cached = await _db.VersionInfos.FirstOrDefaultAsync(ct);
		if (cached != null)
			return cached;

		return await RefreshVersionAsync(ct);
	}

	// ─── 统计数据 ───

	public record StatisticWithVersion(
		Statistic Statistic,
		ServerVersion Version
	);

	/// <summary>
	/// 获取统计数据，附带缓存版本
	/// </summary>
	public async Task<StatisticWithVersion> GetStatisticsAsync(string slug, CancellationToken ct = default)
	{
		var version = await GetCachedVersionAsync(ct);
		var statsResp = await _wfm.GetStatisticsAsync(slug, ct);
		// statsResp 是 Response<Statistic>，取 Data
		var stats = statsResp.Data ?? throw new InvalidOperationException($"统计数据为空");
		return new StatisticWithVersion(stats, version);
	}
}
