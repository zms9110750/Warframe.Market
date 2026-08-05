using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Orders;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>
/// 验证合并后方法（slug 与 id 共用同一 slug 路径）生成的请求 URL。
/// 每个方法都用 slug 和 id 两种语义的输入调用，断言输出 URL 一致。
/// </summary>
public class UrlGenerationTests
{
    private static (FakeHttpMessageHandler Handler, WarframeMarketClient Client) CreatePair()
    {
        var handler = new FakeHttpMessageHandler();
        var http = new HttpClient(handler) {
            BaseAddress = new Uri("https://api.warframe.market")
        };
        return (handler, new WarframeMarketClient(http));
    }

    [Theory]
    [InlineData("prisma-kronen")]          // slug 语义
    [InlineData("54aae292e7798909064f1575")] // itemId 语义
    public async Task GetItemAsync_uses_slug_path(string key)
    {
        var (handler, client) = CreatePair();
        handler.Map($"/v2/item/{key}", Data.File("items", "item.json"));

        await client.GetItemAsync(key);

        Assert.Equal($"/v2/item/{key}", handler.LastRequestUri!.AbsolutePath);
    }

    [Theory]
    [InlineData("prisma-kronen")]
    [InlineData("54aae292e7798909064f1575")]
    public async Task GetItemSetAsync_uses_slug_path(string key)
    {
        var (handler, client) = CreatePair();
        handler.Map($"/v2/item/{key}/set", Data.File("items", "item-set.json"));

        await client.GetItemSetAsync(key);

        Assert.Equal($"/v2/item/{key}/set", handler.LastRequestUri!.AbsolutePath);
    }

    [Theory]
    [InlineData("prisma-kronen")]
    [InlineData("54aae292e7798909064f1575")]
    public async Task GetOrdersItemAsync_uses_slug_path(string key)
    {
        var (handler, client) = CreatePair();
        handler.Map($"/v2/orders/item/{key}", Data.File("orders", "orders-item.json"));

        await client.GetOrdersItemAsync(key);

        Assert.Equal($"/v2/orders/item/{key}", handler.LastRequestUri!.AbsolutePath);
    }

    [Theory]
    [InlineData("prisma-kronen")]
    [InlineData("54aae292e7798909064f1575")]
    public async Task GetOrdersItemTopAsync_uses_slug_path(string key)
    {
        var (handler, client) = CreatePair();
        handler.Map($"/v2/orders/item/{key}/top", Data.File("orders", "top.json"));

        await client.GetOrdersItemTopAsync(key, query: null);

        Assert.Equal($"/v2/orders/item/{key}/top", handler.LastRequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetOrdersItemTopAsync_serializes_query_parameters()
    {
        var (handler, client) = CreatePair();
        handler.Map("/v2/orders/item/prisma-kronen/top", Data.File("orders", "top.json"));

        await client.GetOrdersItemTopAsync("prisma-kronen",
            new OrderTopQueryParameter(Rank: null, RankLt: 5, Charges: null, ChargesLt: null,
                AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null,
                Subtype: "blueprint"));

        Assert.Equal("/v2/orders/item/prisma-kronen/top", handler.LastRequestUri!.AbsolutePath);
        var query = handler.LastRequestUri!.Query;
        Assert.Contains("rankLt=5", query);
        Assert.Contains("subtype=blueprint", query);
    }

    [Theory]
    [InlineData("NadeshikoA")]
    [InlineData("5b2b7b8e0f6a1b3c4d5e6f70")]
    public async Task GetOrdersFromUserAsync_uses_slug_path(string key)
    {
        var (handler, client) = CreatePair();
        handler.Map($"/v2/orders/user/{key}", Data.File("orders", "orders-user.json"));

        await client.GetOrdersFromUserAsync(key);

        Assert.Equal($"/v2/orders/user/{key}", handler.LastRequestUri!.AbsolutePath);
    }

    [Theory]
    [InlineData("NadeshikoA")]
    [InlineData("5b2b7b8e0f6a1b3c4d5e6f70")]
    public async Task GetUserAsync_uses_slug_path(string key)
    {
        var (handler, client) = CreatePair();
        handler.Map($"/v2/user/{key}", Data.File("users", "user.json"));

        await client.GetUserAsync(key);

        Assert.Equal($"/v2/user/{key}", handler.LastRequestUri!.AbsolutePath);
    }

    [Theory]
    [InlineData("NadeshikoA")]
    [InlineData("5b2b7b8e0f6a1b3c4d5e6f70")]
    public async Task GetUserAchievementsAsync_uses_slug_path(string key)
    {
        var (handler, client) = CreatePair();
        handler.Map($"/v2/achievements/user/{key}", Data.File("achievements", "user.json"));

        await client.GetUserAchievementsAsync(key, featured: null);

        Assert.Equal($"/v2/achievements/user/{key}", handler.LastRequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetUserAchievementsAsync_serializes_featured_flag()
    {
        var (handler, client) = CreatePair();
        handler.Map("/v2/achievements/user/NadeshikoA", Data.File("achievements", "user.json"));

        await client.GetUserAchievementsAsync("NadeshikoA", featured: true);

        // 文档：featured 是 presence flag，值被忽略（?featured=false 也启用过滤），只断言参数发出
        Assert.Contains("featured=", handler.LastRequestUri!.Query);
    }
}
