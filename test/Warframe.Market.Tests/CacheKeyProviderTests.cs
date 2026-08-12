using Axion.Extensions.Polly.Caching.Hybrid;
using Xunit;
using zms9110750.Warframe.Market.GUI.Services;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>
/// 锁定 CacheConfig.CacheKeyProvider 的键格式（query/语言头隔离；平台头不参与——用户决定需重启切换平台）。
/// 同时锁 UserSearch.RefreshUserAsync 手写键与此处的契约。
/// </summary>
public class CacheKeyProviderTests
{
    private static async ValueTask<string> Key(string pathAndQuery, string? lang = null, string? platform = null)
    {
        var ctx = Polly.ResilienceContextPool.Shared.Get();
        try
        {
            var msg = new HttpRequestMessage(HttpMethod.Get, "https://api.warframe.market" + pathAndQuery);
            if (lang != null)
            {
                msg.Headers.Add("Language", lang);
            }

            if (platform != null)
            {
                msg.Headers.Add("Platform", platform);
            }

            ctx.SetRequestMessage(msg);
            return await CacheConfig.CacheKeyProvider(ctx).AsTask();
        }
        finally
        {
            Polly.ResilienceContextPool.Shared.Return(ctx);
        }
    }

    [Fact]
    public async Task Key_includes_query_string()
    {
        var a = await Key("/v2/orders/item/prisma-kronen/top?rankLt=5");
        var b = await Key("/v2/orders/item/prisma-kronen/top?rankLt=3");
        Assert.NotEqual(a, b); // query 隔离，避免 rankLt 串缓存
    }

    [Fact]
    public async Task Key_includes_language_header()
    {
        var zh = await Key("/v2/items", lang: "zh-hans");
        var en = await Key("/v2/items", lang: "en");
        Assert.NotEqual(zh, en); // i18n body 随语言变化
        Assert.Contains("|lang=zh-hans", zh);
    }

    [Fact]
    public async Task Key_excludes_platform_header()
    {
        // 用户决定：平台头不参与缓存键（切换平台需重启生效）——锁定当前语义，防止未来无意改动
        var pc = await Key("/v2/user/x", lang: "zh-hans", platform: "pc");
        var ps = await Key("/v2/user/x", lang: "zh-hans", platform: "ps4");
        Assert.Equal(pc, ps);
    }

    [Fact]
    public async Task Key_includes_method_and_path()
    {
        var items = await Key("/v2/items");
        var orders = await Key("/v2/orders/item/x");
        Assert.NotEqual(items, orders);
    }

    [Fact]
    public async Task Key_matches_refresh_manual_key_contract()
    {
        // UserSearch.RefreshUserAsync 手写键格式：get/https/api.warframe.market/v2/user/{name}|lang={lang}
        var key = await Key("/v2/user/zms9110750", lang: "zh-hans");
        Assert.Equal("get/https/api.warframe.market/v2/user/zms9110750|lang=zh-hans", key);
    }

    [Fact]
    public async Task Key_includes_non_default_port()
    {
        var ctx = Polly.ResilienceContextPool.Shared.Get();
        try
        {
            var msg = new HttpRequestMessage(HttpMethod.Get, "https://api.warframe.market:8443/v2/items");
            ctx.SetRequestMessage(msg);
            var key = await CacheConfig.CacheKeyProvider(ctx).AsTask();
            Assert.Contains(":8443", key);
        }
        finally
        {
            Polly.ResilienceContextPool.Shared.Return(ctx);
        }
    }
}
