namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>统计缓存优先级降级公共 helper（物品搜索/用户搜索共用）：
/// 组件关闭/路由离开时把用过的统计 slug 批量降级（tab 关 → High，路由走 → Normal，可逐出）。</summary>
public static class StatPriority
{
    public static void Demote(IItemSearchService svc, IEnumerable<string> slugs, Microsoft.Extensions.Caching.Memory.CacheItemPriority priority)
    {
        foreach (var slug in slugs)
        {
            svc.SetStatisticPriority(slug, priority);
        }
    }
}
