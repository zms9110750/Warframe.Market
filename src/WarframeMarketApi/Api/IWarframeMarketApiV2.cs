using Refit;
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

namespace zms9110750.WarframeMarketApi.Api;

/// <summary>Warframe Market API - V2 公共端点</summary>
internal interface IWarframeMarketApiV2
{
	/// <summary>获取服务器资源的当前版本号</summary>
	[Get("/v2/versions")]
	Task<IApiResponse<Response<ServerVersion>>> GetVersionsAsync(CancellationToken cancellation = default);

	/// <summary>获取所有可交易物品列表</summary>
	[Get("/v2/items")]
	Task<IApiResponse<Response<ItemShort[]>>> GetItemsAsync(CancellationToken cancellation = default);

	/// <summary>获取指定物品的完整信息（按 slug）</summary>
	[Get("/v2/item/{slug}")]
	Task<IApiResponse<Response<Item>>> GetItemAsync(string slug, CancellationToken cancellation = default);

	/// <summary>获取指定物品的完整信息（按 itemId）</summary>
	[Get("/v2/itemId/{itemId}")]
	Task<IApiResponse<Response<Item>>> GetItemByIdAsync(string itemId, CancellationToken cancellation = default);

	/// <summary>获取物品所在套装信息（按 slug）</summary>
	[Get("/v2/item/{slug}/set")]
	Task<IApiResponse<Response<ItemSet>>> GetItemSetAsync(string slug, CancellationToken cancellation = default);

	/// <summary>获取物品所在套装信息（按 itemId）</summary>
	[Get("/v2/itemId/{itemId}/set")]
	Task<IApiResponse<Response<ItemSet>>> GetItemSetByIdAsync(string itemId, CancellationToken cancellation = default);

	/// <summary>获取所有可交易裂罅武器列表</summary>
	[Get("/v2/riven/weapons")]
	Task<IApiResponse<Response<Riven[]>>> GetRivenWeaponsAsync(CancellationToken cancellation = default);

	/// <summary>获取指定裂罅武器的完整信息</summary>
	[Get("/v2/riven/weapon/{slug}")]
	Task<IApiResponse<Response<Riven>>> GetRivenWeaponAsync(string slug, CancellationToken cancellation = default);

	/// <summary>获取所有裂罅属性列表</summary>
	[Get("/v2/riven/attributes")]
	Task<IApiResponse<Response<RivenAttribute[]>>> GetRivenAttributesAsync(CancellationToken cancellation = default);

	/// <summary>获取所有可交易巫妖武器列表</summary>
	[Get("/v2/lich/weapons")]
	Task<IApiResponse<Response<LichWeapon[]>>> GetLichWeaponsAsync(CancellationToken cancellation = default);

	/// <summary>获取指定巫妖武器的完整信息</summary>
	[Get("/v2/lich/weapon/{slug}")]
	Task<IApiResponse<Response<LichWeapon>>> GetLichWeaponAsync(string slug, CancellationToken cancellation = default);

	/// <summary>获取所有可交易巫妖幻纹列表</summary>
	[Get("/v2/lich/ephemeras")]
	Task<IApiResponse<Response<LichEphemera[]>>> GetLichEphemerasAsync(CancellationToken cancellation = default);

	/// <summary>获取所有巫妖怪癖列表</summary>
	[Get("/v2/lich/quirks")]
	Task<IApiResponse<Response<LichQuirk[]>>> GetLichQuirksAsync(CancellationToken cancellation = default);

	/// <summary>获取所有可交易姐妹武器列表</summary>
	[Get("/v2/sister/weapons")]
	Task<IApiResponse<Response<SisterWeapon[]>>> GetSisterWeaponsAsync(CancellationToken cancellation = default);

	/// <summary>获取指定姐妹武器的完整信息</summary>
	[Get("/v2/sister/weapon/{slug}")]
	Task<IApiResponse<Response<SisterWeapon>>> GetSisterWeaponAsync(string slug, CancellationToken cancellation = default);

	/// <summary>获取所有可交易姐妹幻纹列表</summary>
	[Get("/v2/sister/ephemeras")]
	Task<IApiResponse<Response<SisterEphemera[]>>> GetSisterEphemerasAsync(CancellationToken cancellation = default);

	/// <summary>获取所有姐妹怪癖列表</summary>
	[Get("/v2/sister/quirks")]
	Task<IApiResponse<Response<SisterQuirk[]>>> GetSisterQuirksAsync(CancellationToken cancellation = default);

	/// <summary>获取所有位置节点列表</summary>
	[Get("/v2/locations")]
	Task<IApiResponse<Response<Location[]>>> GetLocationsAsync(CancellationToken cancellation = default);

	/// <summary>获取所有 NPC 列表</summary>
	[Get("/v2/npcs")]
	Task<IApiResponse<Response<Npc[]>>> GetNpcsAsync(CancellationToken cancellation = default);

	/// <summary>获取所有任务列表</summary>
	[Get("/v2/missions")]
	Task<IApiResponse<Response<Mission[]>>> GetMissionsAsync(CancellationToken cancellation = default);

	/// <summary>获取最新订单（最多 500 条，过去 4 小时内）</summary>
	[Get("/v2/orders/recent")]
	Task<IApiResponse<Response<Order[]>>> GetOrdersRecentAsync(CancellationToken cancellation = default);

	/// <summary>获取指定物品的所有订单（按 slug）</summary>
	[Get("/v2/orders/item/{slug}")]
	Task<IApiResponse<Response<Order[]>>> GetOrdersItemAsync(string slug, CancellationToken cancellation = default);

	/// <summary>获取指定物品的所有订单（按 itemId）</summary>
	[Get("/v2/orders/itemId/{itemId}")]
	Task<IApiResponse<Response<Order[]>>> GetOrdersItemByIdAsync(string itemId, CancellationToken cancellation = default);

	/// <summary>获取指定物品在线用户的 Top5 买卖单（按 slug）</summary>
	[Get("/v2/orders/item/{slug}/top")]
	Task<IApiResponse<Response<OrderTop>>> GetOrdersItemTopAsync(string slug, [Query] OrderTopQueryParameter? query = null, CancellationToken cancellation = default);

	/// <summary>获取指定物品在线用户的 Top5 买卖单（按 itemId）</summary>
	[Get("/v2/orders/itemId/{itemId}/top")]
	Task<IApiResponse<Response<OrderTop>>> GetOrdersItemTopByIdAsync(string itemId, [Query] OrderTopQueryParameter? query = null, CancellationToken cancellation = default);

	/// <summary>获取指定用户的公开订单（按 slug）</summary>
	[Get("/v2/orders/user/{slug}")]
	Task<IApiResponse<Response<Order[]>>> GetOrdersFromUserAsync(string slug, CancellationToken cancellation = default);

	/// <summary>获取指定用户的公开订单（按 userId）</summary>
	[Get("/v2/orders/userId/{userId}")]
	Task<IApiResponse<Response<Order[]>>> GetOrdersFromUserIdAsync(string userId, CancellationToken cancellation = default);

	/// <summary>获取指定用户的公开信息（按 slug）</summary>
	[Get("/v2/user/{slug}")]
	Task<IApiResponse<Response<User>>> GetUserAsync(string slug, CancellationToken cancellation = default);

	/// <summary>获取指定用户的公开信息（按 userId）</summary>
	[Get("/v2/userId/{userId}")]
	Task<IApiResponse<Response<User>>> GetUserByIdAsync(string userId, CancellationToken cancellation = default);

	/// <summary>获取所有可用成就列表</summary>
	[Get("/v2/achievements")]
	Task<IApiResponse<Response<Achievement[]>>> GetAchievementsAsync(CancellationToken cancellation = default);

	/// <summary>获取指定用户的成就（按 slug）</summary>
	[Get("/v2/achievements/user/{slug}")]
	Task<IApiResponse<Response<Achievement[]>>> GetUserAchievementsAsync(string slug, [Query] bool? featured = null, CancellationToken cancellation = default);

	/// <summary>获取指定用户的成就（按 userId）</summary>
	[Get("/v2/achievements/userId/{userId}")]
	Task<IApiResponse<Response<Achievement[]>>> GetUserAchievementsByIdAsync(string userId, [Query] bool? featured = null, CancellationToken cancellation = default);

	/// <summary>获取移动端主页展示面板</summary>
	[Get("/v2/dashboard/showcase")]
	Task<IApiResponse<Response<DashboardShowcase>>> GetDashboardShowcaseAsync(CancellationToken cancellation = default);
}
