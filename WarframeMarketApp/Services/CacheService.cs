using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Versions;
using WarframeMarketApp.Data;

namespace WarframeMarketApp.Services;

/// <summary>
/// 缓存管理：启动延迟清理 + 版本按钮完整语义。
/// 单例，DB 访问通过 IServiceScopeFactory。
/// </summary>
public class CacheService
{
	private readonly WarframeMarketClient _wfm;
	private readonly IServiceScopeFactory _scopeFactory;
	private static readonly Random _rng = new();

	public CacheService(WarframeMarketClient wfm, IServiceScopeFactory scopeFactory)
	{
		_wfm = wfm;
		_scopeFactory = scopeFactory;
	}

	// ─── 启动后延迟清理 ───

	public async Task StartupCleanupAsync(CancellationToken ct = default)
	{
		var delaySec = _rng.Next(3, 11);
		try { await Task.Delay(TimeSpan.FromSeconds(delaySec), ct); }
		catch (OperationCanceledException) { return; }

		try
		{
			using var scope = _scopeFactory.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<WfmDbContext>();
			var cutoff = DateTime.UtcNow.Date.AddDays(-2);
			await db.Database.ExecuteSqlRawAsync(
				"DELETE FROM Cache WHERE CachedAt < @p0", cutoff);
		}
		catch { }
	}

	// ─── 版本与数据刷新 ───

	public record VersionStatus(string? VersionId, string? UpdatedAt, bool HasLocalData);

	public async Task<VersionStatus> GetLocalStatusAsync(CancellationToken ct = default)
	{
		using var scope = _scopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<WfmDbContext>();
		var cached = await db.VersionInfos.FirstOrDefaultAsync(ct);
		if (cached == null)
			return new(null, null, false);
		return new(cached.Id, cached.UpdatedAt, true);
	}

	public async Task<ServerVersion?> GetServerVersionAsync(CancellationToken ct = default)
	{
		var resp = await _wfm.GetVersionsAsync(ct);
		return resp?.Content?.Data;
	}

	/// <summary>删表 → 拉取 → 写入 SQLite → 写版本</summary>
	public async Task RefreshAllAsync(CancellationToken ct = default)
	{
		using var scope = _scopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<WfmDbContext>();

		await db.Database.ExecuteSqlRawAsync("DELETE FROM ItemTranslations");
		await db.Database.ExecuteSqlRawAsync("DELETE FROM Items");
		await db.Database.ExecuteSqlRawAsync("DELETE FROM VersionInfos");
		await db.Database.ExecuteSqlRawAsync("DELETE FROM Cache");

		var resp = await _wfm.GetItemsAsync(ct);
		var items = resp?.Content?.Data;
		if (items == null || items.Length == 0)
			throw new InvalidOperationException("从 API 获取物品列表失败");

		db.Items.AddRange(items);
		foreach (var item in items)
		{
			foreach (var (lang, pake) in item.I18n)
			{
				db.ItemTranslations.Add(new ItemTranslation(
					 item.Id, lang.ToString(),
					 pake.Name, pake.Description, pake.WikiLink,
					 pake.Icon, pake.Thumb, pake.SubIcon));
			}
		}
		await db.SaveChangesAsync(ct);

		var serverVersion = await GetServerVersionAsync(ct);
		if (serverVersion != null)
		{
			db.VersionInfos.Add(serverVersion);
			await db.SaveChangesAsync(ct);
		}
	}

	// ─── 统计数据 ───

	public async Task<zms9110750.WarframeMarketApi.Models.Statistics.Statistic?> GetStatisticsAsync(
		string itemId, CancellationToken ct = default)
	{
		var resp = await _wfm.GetStatisticsAsync(itemId, ct);
		return resp?.Data;
	}
}
