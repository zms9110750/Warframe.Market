using System.Text.Json;
using Refit;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models;
using zms9110750.WarframeMarketApi.Models.Achievements;
using zms9110750.WarframeMarketApi.Models.Dashboard;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Liches;
using zms9110750.WarframeMarketApi.Models.Locations;
using zms9110750.WarframeMarketApi.Models.Missions;
using zms9110750.WarframeMarketApi.Models.Npcs;
using zms9110750.WarframeMarketApi.Models.Orders;
using zms9110750.WarframeMarketApi.Models.Rivens;
using zms9110750.WarframeMarketApi.Models.Sisters;
using zms9110750.WarframeMarketApi.Models.Users;
using zms9110750.WarframeMarketApi.Models.Versions;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>
/// 用 test/ 下备份的真实 API JSON 假数据验证所有公共端点的反序列化，
/// 覆盖全部模型家族，确保模型与线上响应形状一致。
/// </summary>
public class DeserializationTests
{
    private static (FakeHttpMessageHandler Handler, WarframeMarketClient Client) CreatePair()
    {
        var handler = new FakeHttpMessageHandler();
        var http = new HttpClient(handler) {
            BaseAddress = new Uri("https://api.warframe.market")
        };
        return (handler, new WarframeMarketClient(http));
    }

    private static async Task<Response<T>> FetchAsync<T>(
        Action<FakeHttpMessageHandler> map, Func<WarframeMarketClient, Task<IApiResponse<Response<T>>>> call)
    {
        var (handler, client) = CreatePair();
        map(handler);
        var response = await call(client);
        Assert.True(response.IsSuccessStatusCode, "HTTP 非 2xx");
        Assert.NotNull(response.Content);
        return response.Content;
    }

    /// <summary>从备份的列表 JSON 中读取第一个元素的 slug（与 backup-data.ps1 同步）</summary>
    private static string FirstSlug(string resource, string file)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Data.File(resource, file)));
        return doc.RootElement.GetProperty("data")[0].GetProperty("slug").GetString()!;
    }

    /// <summary>从备份的 recent 订单中读取一个在线用户 slug</summary>
    private static string FirstUserSlug()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Data.File("orders", "recent.json")));
        return doc.RootElement.GetProperty("data")[0].GetProperty("user").GetProperty("slug").GetString()!;
    }

    [Fact]
    public async Task Versions_roundtrip()
    {
        var content = await FetchAsync(
            h => h.Map("/v2/versions", Data.File("versions", "versions.json")),
            c => c.GetVersionsAsync());

        Assert.False(string.IsNullOrEmpty(content.ApiVersion));
        Assert.NotNull(content.Data);
        Assert.False(string.IsNullOrEmpty(content.Data.Id));
    }

    [Fact]
    public async Task Items_list_roundtrip()
    {
        var content = await FetchAsync(
            h => h.Map("/v2/items", Data.File("items", "items.json")),
            c => c.GetItemsAsync());

        Assert.NotEmpty(content.Data);
        Assert.False(string.IsNullOrEmpty(content.Data[0].Id));
        Assert.False(string.IsNullOrEmpty(content.Data[0].Slug));
    }

    [Fact]
    public async Task Item_roundtrip()
    {
        var slug = FirstSlug("items", "items.json");
        var content = await FetchAsync(
            h => h.Map($"/v2/item/{slug}", Data.File("items", "item.json")),
            c => c.GetItemAsync(slug));

        Assert.False(string.IsNullOrEmpty(content.Data.Id));
        Assert.Equal(slug, content.Data.Slug);
    }

    [Fact]
    public async Task Item_set_roundtrip()
    {
        var slug = FirstSlug("items", "items.json");
        var content = await FetchAsync(
            h => h.Map($"/v2/item/{slug}/set", Data.File("items", "item-set.json")),
            c => c.GetItemSetAsync(slug));

        Assert.False(string.IsNullOrEmpty(content.Data.Id));
        Assert.NotEmpty(content.Data.Items);
    }

    [Fact]
    public async Task Riven_weapons_roundtrip()
    {
        var content = await FetchAsync(
            h => h.Map("/v2/riven/weapons", Data.File("rivens", "weapons.json")),
            c => c.GetRivenWeaponsAsync());

        Assert.NotEmpty(content.Data);
        Assert.False(string.IsNullOrEmpty(content.Data[0].Slug));
    }

    [Fact]
    public async Task Riven_weapon_roundtrip()
    {
        var slug = FirstSlug("rivens", "weapons.json");
        var content = await FetchAsync(
            h => h.Map($"/v2/riven/weapon/{slug}", Data.File("rivens", "weapon.json")),
            c => c.GetRivenWeaponAsync(slug));

        Assert.False(string.IsNullOrEmpty(content.Data.Slug));
    }

    [Fact]
    public async Task Riven_attributes_roundtrip()
    {
        var content = await FetchAsync(
            h => h.Map("/v2/riven/attributes", Data.File("rivens", "attributes.json")),
            c => c.GetRivenAttributesAsync());

        Assert.NotEmpty(content.Data);
    }

    [Fact]
    public async Task Lich_weapons_roundtrip()
    {
        var content = await FetchAsync(
            h => h.Map("/v2/lich/weapons", Data.File("liches", "weapons.json")),
            c => c.GetLichWeaponsAsync());

        Assert.NotEmpty(content.Data);
        Assert.False(string.IsNullOrEmpty(content.Data[0].Slug));
    }

    [Fact]
    public async Task Lich_weapon_roundtrip()
    {
        var slug = FirstSlug("liches", "weapons.json");
        var content = await FetchAsync(
            h => h.Map($"/v2/lich/weapon/{slug}", Data.File("liches", "weapon.json")),
            c => c.GetLichWeaponAsync(slug));

        Assert.False(string.IsNullOrEmpty(content.Data.Slug));
    }

    [Fact]
    public async Task Lich_ephemeras_roundtrip()
    {
        var content = await FetchAsync(
            h => h.Map("/v2/lich/ephemeras", Data.File("liches", "ephemeras.json")),
            c => c.GetLichEphemerasAsync());

        Assert.NotEmpty(content.Data);
    }

    [Fact]
    public async Task Lich_quirks_roundtrip()
    {
        var content = await FetchAsync(
            h => h.Map("/v2/lich/quirks", Data.File("liches", "quirks.json")),
            c => c.GetLichQuirksAsync());

        Assert.NotEmpty(content.Data);
    }

    [Fact]
    public async Task Sister_weapons_roundtrip()
    {
        var content = await FetchAsync(
            h => h.Map("/v2/sister/weapons", Data.File("sisters", "weapons.json")),
            c => c.GetSisterWeaponsAsync());

        Assert.NotEmpty(content.Data);
        Assert.False(string.IsNullOrEmpty(content.Data[0].Slug));
    }

    [Fact]
    public async Task Sister_weapon_roundtrip()
    {
        var slug = FirstSlug("sisters", "weapons.json");
        var content = await FetchAsync(
            h => h.Map($"/v2/sister/weapon/{slug}", Data.File("sisters", "weapon.json")),
            c => c.GetSisterWeaponAsync(slug));

        Assert.False(string.IsNullOrEmpty(content.Data.Slug));
    }

    [Fact]
    public async Task Sister_ephemeras_roundtrip()
    {
        var content = await FetchAsync(
            h => h.Map("/v2/sister/ephemeras", Data.File("sisters", "ephemeras.json")),
            c => c.GetSisterEphemerasAsync());

        Assert.NotEmpty(content.Data);
    }

    [Fact]
    public async Task Sister_quirks_roundtrip()
    {
        var content = await FetchAsync(
            h => h.Map("/v2/sister/quirks", Data.File("sisters", "quirks.json")),
            c => c.GetSisterQuirksAsync());

        Assert.NotEmpty(content.Data);
    }

    [Fact]
    public async Task Locations_roundtrip()
    {
        var content = await FetchAsync(
            h => h.Map("/v2/locations", Data.File("locations", "locations.json")),
            c => c.GetLocationsAsync());

        Assert.NotEmpty(content.Data);
        Assert.False(string.IsNullOrEmpty(content.Data[0].Slug));
    }

    [Fact]
    public async Task Npcs_roundtrip()
    {
        var content = await FetchAsync(
            h => h.Map("/v2/npcs", Data.File("npcs", "npcs.json")),
            c => c.GetNpcsAsync());

        Assert.NotEmpty(content.Data);
        Assert.False(string.IsNullOrEmpty(content.Data[0].Slug));
    }

    [Fact]
    public async Task Missions_roundtrip()
    {
        var content = await FetchAsync(
            h => h.Map("/v2/missions", Data.File("missions", "missions.json")),
            c => c.GetMissionsAsync());

        Assert.NotEmpty(content.Data);
        Assert.False(string.IsNullOrEmpty(content.Data[0].Slug));
    }

    [Fact]
    public async Task Orders_recent_roundtrip()
    {
        var content = await FetchAsync(
            h => h.Map("/v2/orders/recent", Data.File("orders", "recent.json")),
            c => c.GetOrdersRecentAsync());

        Assert.NotEmpty(content.Data);
        Assert.False(string.IsNullOrEmpty(content.Data[0].Id));
        Assert.False(string.IsNullOrEmpty(content.Data[0].ItemId));
    }

    [Fact]
    public async Task Orders_item_roundtrip()
    {
        var slug = FirstSlug("items", "items.json");
        var content = await FetchAsync(
            h => h.Map($"/v2/orders/item/{slug}", Data.File("orders", "orders-item.json")),
            c => c.GetOrdersItemAsync(slug));

        Assert.NotEmpty(content.Data);
        Assert.False(string.IsNullOrEmpty(content.Data[0].Id));
    }

    [Fact]
    public async Task Orders_top_roundtrip()
    {
        var slug = FirstSlug("items", "items.json");
        var content = await FetchAsync(
            h => h.Map($"/v2/orders/item/{slug}/top", Data.File("orders", "top.json")),
            c => c.GetOrdersItemTopAsync(slug, query: null));

        Assert.NotNull(content.Data.Sell);
        Assert.NotNull(content.Data.Buy);
    }

    [Fact]
    public async Task Orders_user_roundtrip()
    {
        var userSlug = FirstUserSlug();
        var content = await FetchAsync(
            h => h.Map($"/v2/orders/user/{userSlug}", Data.File("orders", "orders-user.json")),
            c => c.GetOrdersFromUserAsync(userSlug));

        Assert.NotEmpty(content.Data);
    }

    [Fact]
    public async Task User_roundtrip()
    {
        var userSlug = FirstUserSlug();
        var content = await FetchAsync(
            h => h.Map($"/v2/user/{userSlug}", Data.File("users", "user.json")),
            c => c.GetUserAsync(userSlug));

        Assert.False(string.IsNullOrEmpty(content.Data.Slug));
        Assert.False(string.IsNullOrEmpty(content.Data.IngameName));
    }

    [Fact]
    public async Task Achievements_roundtrip()
    {
        var content = await FetchAsync(
            h => h.Map("/v2/achievements", Data.File("achievements", "achievements.json")),
            c => c.GetAchievementsAsync());

        Assert.NotEmpty(content.Data);
        Assert.False(string.IsNullOrEmpty(content.Data[0].Slug));
    }

    [Fact]
    public async Task User_achievements_roundtrip()
    {
        // 备份用户可能没有任何成就（data 为空数组），此处只断言解析成功
        var userSlug = FirstUserSlug();
        var content = await FetchAsync(
            h => h.Map($"/v2/achievements/user/{userSlug}", Data.File("achievements", "user.json")),
            c => c.GetUserAchievementsAsync(userSlug, featured: null));

        Assert.NotNull(content.Data);
    }

    [Fact]
    public async Task Dashboard_showcase_roundtrip()
    {
        var content = await FetchAsync(
            h => h.Map("/v2/dashboard/showcase", Data.File("dashboard", "showcase.json")),
            c => c.GetDashboardShowcaseAsync());

        Assert.NotNull(content.Data);
        Assert.NotEmpty(content.Data.Items);
    }
}
