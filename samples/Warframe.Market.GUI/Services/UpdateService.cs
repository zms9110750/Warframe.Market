using Serilog;
using zms9110750.Warframe.Market.GUI.Api;

namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>
/// Gitee 更新检查：版本列表经 FusionCache 缓存（跳过分布式层，仅进程内）
/// </summary>
public class UpdateService
{
    public const string Owner = "zms9110750";
    public const string Repo = "Warframe.Market";

    private readonly IGitee _gitee;
    private readonly ZiggyCreatures.Caching.Fusion.IFusionCache _cache;

    public UpdateService(IGitee gitee, ZiggyCreatures.Caching.Fusion.IFusionCache cache)
    {
        _gitee = gitee;
        _cache = cache;
    }

    public async Task<GiteeRelease[]> GetReleasesAsync()
    {
        Log.Information("UpdateService 获取版本列表");
        return await _cache.GetOrSetAsync(nameof(GetReleasesAsync), async _ =>
            await _gitee.ReleasesAsync(Owner, Repo, direction: "desc"),
            op => op.SetSkipDistributedCache(true, null));
    }
}
