using Bunit;
using Masa.Blazor;
using Xunit;
using zms9110750.WarframeMarketApi.Models.Arcane;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Statistics;
using zms9110750.Warframe.Market.GUI.Services;
using ArcanePacksPanel = zms9110750.Warframe.Market.GUI.Pages.ArcanePacks;
using ArcaneTablePanel = zms9110750.Warframe.Market.GUI.Pages.ArcaneTable;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>
/// 赋能包失败态测试：主表任务失败 → "-"（非 0.0 假数值）；明细单物品失败 → 该行 "-" 且不整体抛错。
/// </summary>
public class ArcaneFailureTests
{
    private sealed class FakeExternalLink : zms9110750.Warframe.Market.GUI.Services.IExternalLinkService
    {
        public void Open(string url) { }
    }

    /// <summary>测试专用配置目录（临时，不触碰真实 %LocalAppData%\WarframeMarket）</summary>
    private static string TestConfigDir()
    {
        return Path.Combine(Path.GetTempPath(), $"wm-config-test-{Guid.NewGuid():N}");
    }

    /// <summary>假 IArcanePackService：GetReferencePriceAsync 抛异常（模拟统计失败）</summary>
    private sealed class ThrowingArcanePack : IArcanePackService
    {
        public Task<double> GetReferencePriceAsync(ArcanePackConfig pack, int purchase = 0)
        {
            throw new HttpRequestException("模拟统计失败");
        }

        public double GetDailyVolume(Statistic? stat)
        {
            return 0;
        }

        public void SetStatisticsPriority(Microsoft.Extensions.Caching.Memory.CacheItemPriority priority)
        {
        }
    }

    /// <summary>假 IItemSearchService：FindByKeyAsync 返回物品，GetStatisticAsync 抛异常</summary>
    private sealed class ThrowingItemSearch : IItemSearchService
    {
        public Task<List<ItemShort>> SearchAsync(string query, CancellationToken ct = default)
        {
            return Task.FromResult(new List<ItemShort>());
        }

        public Task<ItemShort?> FindByKeyAsync(string key)
        {
            return Task.FromResult<ItemShort?>(new ItemShort(
                "item1", "slug1", "GameRef", new HashSet<string>(), 5,
                null, null, null, null, null, null, null,
                new Dictionary<Language, LanguagePake>()));
        }

        public Task<Statistic?> GetStatisticAsync(string slug, CancellationToken ct = default)
        {
            throw new HttpRequestException("模拟统计失败");
        }

        public void Invalidate()
        {
        }

        public double? GetReferencePrice(Statistic? stat)
        {
            return stat?.GetReferencePrice();
        }

        public double? GetMaxReferencePrice(Statistic? stat)
        {
            return stat?.GetMaxReferencePrice();
        }

        public void SetStatisticPriority(string slug, Microsoft.Extensions.Caching.Memory.CacheItemPriority priority)
        {
        }
    }

    private static ArcanePackConfig MakePack()
    {
        return new ArcanePackConfig {
            Name = "测试包",
            Items =
            [
                new ArcaneQualityGroup { Subtypes = "Common", Quality = 1, Items = ["测试物品"] },
            ],
        };
    }

    [Fact]
    public async Task ArcanePacks_failure_renders_dash_not_zero()
    {
        // 主表：统计失败 → 任务完成且 Result null → 渲染 "-"（不是 0.0 假数值）
        await using var ctx = new BunitContext();
        ctx.Services.AddMasaBlazor();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        // 注入临时目录的 ConfigService——测试不触碰真实 %LocalAppData%\WarframeMarket
        ctx.Services.AddSingleton<zms9110750.Warframe.Market.GUI.Services.IConfigService>(new zms9110750.Warframe.Market.GUI.Services.ConfigService(TestConfigDir()));
        ctx.Services.AddSingleton<zms9110750.Warframe.Market.GUI.Services.IAppStateService>(_ => new zms9110750.Warframe.Market.GUI.Services.AppState(new zms9110750.WarframeMarketApi.WarframeMarketClient(new HttpClient())));
        ctx.Services.AddSingleton<zms9110750.Warframe.Market.GUI.Services.IExternalLinkService>(_ => new FakeExternalLink()); // 真（测试 bin 有 赋能包配置.yaml）
        ctx.Services.AddSingleton<IArcanePackService>(new ThrowingArcanePack());

        var cut = ctx.Render<ArcanePacksPanel>();

        // 等待 45 任务完成（假服务抛 → ComputeAsync catch → null → "-"）
        cut.WaitForAssertion(() => {
            Assert.Contains("-", cut.Markup);
            Assert.DoesNotContain("0.0", cut.Markup); // 失败不显示 0.0 假数值
        }, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ArcaneTable_failure_renders_dash_without_crash()
    {
        // 明细：单物品统计失败 → 该行 "-"，且不整体抛错（catch 写 null 而非 throw）
        await using var ctx = new BunitContext();
        ctx.Services.AddMasaBlazor();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddSingleton<zms9110750.Warframe.Market.GUI.Services.IAppStateService>(_ => new zms9110750.Warframe.Market.GUI.Services.AppState(new zms9110750.WarframeMarketApi.WarframeMarketClient(new HttpClient())));
        ctx.Services.AddSingleton<zms9110750.Warframe.Market.GUI.Services.IExternalLinkService>(_ => new FakeExternalLink());
        ctx.Services.AddSingleton<IItemSearchService>(new ThrowingItemSearch());

        var cut = ctx.Render<ArcaneTablePanel>(p => p.Add(m => m.Pack, MakePack()));

        // 出货率%有值；参考价/出货率×价/日均 → "-"（失败写 null）
        Assert.Contains("测试物品", cut.Markup);
        Assert.Contains("-", cut.Markup);
    }

    [Fact]
    public async Task ArcanePacks_dispose_calls_set_statistics_priority_normal()
    {
        // 路由离开（组件 Dispose）→ 本次用过的统计降级为 Normal（可逐出）
        await using var ctx = new BunitContext();
        ctx.Services.AddMasaBlazor();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddSingleton<zms9110750.Warframe.Market.GUI.Services.IConfigService>(new zms9110750.Warframe.Market.GUI.Services.ConfigService(TestConfigDir()));
        ctx.Services.AddSingleton<zms9110750.Warframe.Market.GUI.Services.IAppStateService>(_ => new zms9110750.Warframe.Market.GUI.Services.AppState(new zms9110750.WarframeMarketApi.WarframeMarketClient(new HttpClient())));
        ctx.Services.AddSingleton<zms9110750.Warframe.Market.GUI.Services.IExternalLinkService>(_ => new FakeExternalLink());
        var recording = new RecordingArcanePack();
        ctx.Services.AddSingleton<IArcanePackService>(recording);

        var cut = ctx.Render<ArcanePacksPanel>();
        cut.Instance.Dispose(); // 组件路由离开 → Dispose → SetStatisticsPriority(Normal)

        Assert.True(recording.SetPriorityCalled);
        Assert.Equal(Microsoft.Extensions.Caching.Memory.CacheItemPriority.Normal, recording.LastPriority);
    }

    [Fact]
    public async Task ArcanePackService_set_statistics_priority_demotes_used_slugs()
    {
        // 服务层：GetReferencePriceAsync 记录用过的 slug → SetStatisticsPriority(Normal) 批量降级
        var items = new RecordingItemSearch();
        var svc = new ArcanePackService(items);

        await svc.GetReferencePriceAsync(MakePack()); // 内部 FindByKeyAsync → slug1

        svc.SetStatisticsPriority(Microsoft.Extensions.Caching.Memory.CacheItemPriority.Normal);

        Assert.Contains(("slug1", Microsoft.Extensions.Caching.Memory.CacheItemPriority.Normal), items.Demoted);
    }

    /// <summary>假 IArcanePackService：记录 SetStatisticsPriority 调用</summary>
    private sealed class RecordingArcanePack : IArcanePackService
    {
        public bool SetPriorityCalled { get; private set; }
        public Microsoft.Extensions.Caching.Memory.CacheItemPriority LastPriority { get; private set; }

        public Task<double> GetReferencePriceAsync(ArcanePackConfig pack, int purchase = 0)
        {
            return Task.FromResult(10.0);
        }

        public double GetDailyVolume(Statistic? stat)
        {
            return 0;
        }

        public void SetStatisticsPriority(Microsoft.Extensions.Caching.Memory.CacheItemPriority priority)
        {
            SetPriorityCalled = true;
            LastPriority = priority;
        }
    }

    /// <summary>假 IItemSearchService：记录 SetStatisticPriority(slug, priority) 调用，FindByKeyAsync 返回 slug1</summary>
    private sealed class RecordingItemSearch : IItemSearchService
    {
        public List<(string Slug, Microsoft.Extensions.Caching.Memory.CacheItemPriority Priority)> Demoted { get; } = new();

        public Task<List<ItemShort>> SearchAsync(string query, CancellationToken ct = default)
        {
            return Task.FromResult(new List<ItemShort>());
        }

        public Task<ItemShort?> FindByKeyAsync(string key)
        {
            return Task.FromResult<ItemShort?>(new ItemShort(
                "item1", "slug1", "GameRef", new HashSet<string>(), 5,
                null, null, null, null, null, null, null,
                new Dictionary<Language, LanguagePake>()));
        }

        public Task<Statistic?> GetStatisticAsync(string slug, CancellationToken ct = default)
        {
            return Task.FromResult<Statistic?>(null);
        }

        public void Invalidate()
        {
        }

        public double? GetReferencePrice(Statistic? stat)
        {
            return stat?.GetReferencePrice();
        }

        public double? GetMaxReferencePrice(Statistic? stat)
        {
            return stat?.GetMaxReferencePrice();
        }

        public void SetStatisticPriority(string slug, Microsoft.Extensions.Caching.Memory.CacheItemPriority priority)
        {
            Demoted.Add((slug, priority));
        }
    }
}
