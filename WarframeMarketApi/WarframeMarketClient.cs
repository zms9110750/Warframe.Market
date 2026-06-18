using Refit;
using zms9110750.WarframeMarketApi.Api;
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
using zms9110750.WarframeMarketApi.Models.Statistics;
using zms9110750.WarframeMarketApi.Models.Users;
using Version = zms9110750.WarframeMarketApi.Models.Versions.Version;

namespace zms9110750.WarframeMarketApi;

/// <summary>
/// Warframe.Market API 客户端。
/// 实现 <see cref="IWarframeMarketApiV2"/> 所有公共端点，并额外提供 V1 统计数据的 V2 包装。
/// </summary>
public class WarframeMarketClient : IWarframeMarketApiV2
{
	private readonly IWarframeMarketApiV2 _apiV2;
	private readonly IWarframeMarketApiV1 _apiV1;

	public WarframeMarketClient(IWarframeMarketApiV2 apiV2, IWarframeMarketApiV1 apiV1)
	{
		_apiV2 = apiV2;
		_apiV1 = apiV1;
	}

	public Task<IApiResponse<Response<Version>>> GetVersionsAsync(CancellationToken cancellation) =>
		_apiV2.GetVersionsAsync(cancellation);
	public Task<IApiResponse<Response<ItemShort[]>>> GetItemsAsync(CancellationToken cancellation) =>
		_apiV2.GetItemsAsync(cancellation);
	public Task<IApiResponse<Response<Item>>> GetItemAsync(string slug, CancellationToken cancellation) =>
		_apiV2.GetItemAsync(slug, cancellation);
	public Task<IApiResponse<Response<Item>>> GetItemByIdAsync(string itemId, CancellationToken cancellation) =>
		_apiV2.GetItemByIdAsync(itemId, cancellation);
	public Task<IApiResponse<Response<ItemSet>>> GetItemSetAsync(string slug, CancellationToken cancellation) =>
		_apiV2.GetItemSetAsync(slug, cancellation);
	public Task<IApiResponse<Response<ItemSet>>> GetItemSetByIdAsync(string itemId, CancellationToken cancellation) =>
		_apiV2.GetItemSetByIdAsync(itemId, cancellation);
	public Task<IApiResponse<Response<Riven[]>>> GetRivenWeaponsAsync(CancellationToken cancellation) =>
		_apiV2.GetRivenWeaponsAsync(cancellation);
	public Task<IApiResponse<Response<Riven>>> GetRivenWeaponAsync(string slug, CancellationToken cancellation) =>
		_apiV2.GetRivenWeaponAsync(slug, cancellation);
	public Task<IApiResponse<Response<RivenAttribute[]>>> GetRivenAttributesAsync(CancellationToken cancellation) =>
		_apiV2.GetRivenAttributesAsync(cancellation);
	public Task<IApiResponse<Response<LichWeapon[]>>> GetLichWeaponsAsync(CancellationToken cancellation) =>
		_apiV2.GetLichWeaponsAsync(cancellation);
	public Task<IApiResponse<Response<LichWeapon>>> GetLichWeaponAsync(string slug, CancellationToken cancellation) =>
		_apiV2.GetLichWeaponAsync(slug, cancellation);
	public Task<IApiResponse<Response<LichEphemera[]>>> GetLichEphemerasAsync(CancellationToken cancellation) =>
		_apiV2.GetLichEphemerasAsync(cancellation);
	public Task<IApiResponse<Response<LichQuirk[]>>> GetLichQuirksAsync(CancellationToken cancellation) =>
		_apiV2.GetLichQuirksAsync(cancellation);
	public Task<IApiResponse<Response<SisterWeapon[]>>> GetSisterWeaponsAsync(CancellationToken cancellation) =>
		_apiV2.GetSisterWeaponsAsync(cancellation);
	public Task<IApiResponse<Response<SisterWeapon>>> GetSisterWeaponAsync(string slug, CancellationToken cancellation) =>
		_apiV2.GetSisterWeaponAsync(slug, cancellation);
	public Task<IApiResponse<Response<SisterEphemera[]>>> GetSisterEphemerasAsync(CancellationToken cancellation) =>
		_apiV2.GetSisterEphemerasAsync(cancellation);
	public Task<IApiResponse<Response<SisterQuirk[]>>> GetSisterQuirksAsync(CancellationToken cancellation) =>
		_apiV2.GetSisterQuirksAsync(cancellation);
	public Task<IApiResponse<Response<Location[]>>> GetLocationsAsync(CancellationToken cancellation) =>
		_apiV2.GetLocationsAsync(cancellation);
	public Task<IApiResponse<Response<Npc[]>>> GetNpcsAsync(CancellationToken cancellation) =>
		_apiV2.GetNpcsAsync(cancellation);
	public Task<IApiResponse<Response<Mission[]>>> GetMissionsAsync(CancellationToken cancellation) =>
		_apiV2.GetMissionsAsync(cancellation);
	public Task<IApiResponse<Response<Order[]>>> GetOrdersRecentAsync(CancellationToken cancellation) =>
		_apiV2.GetOrdersRecentAsync(cancellation);
	public Task<IApiResponse<Response<Order[]>>> GetOrdersItemAsync(string slug, CancellationToken cancellation) =>
		_apiV2.GetOrdersItemAsync(slug, cancellation);
	public Task<IApiResponse<Response<Order[]>>> GetOrdersItemByIdAsync(string itemId, CancellationToken cancellation) =>
		_apiV2.GetOrdersItemByIdAsync(itemId, cancellation);
	public Task<IApiResponse<Response<OrderTop>>> GetOrdersItemTopAsync(string slug, OrderTopQueryParameter? query, CancellationToken cancellation) =>
		_apiV2.GetOrdersItemTopAsync(slug, query, cancellation);
	public Task<IApiResponse<Response<OrderTop>>> GetOrdersItemTopByIdAsync(string itemId, OrderTopQueryParameter? query, CancellationToken cancellation) =>
		_apiV2.GetOrdersItemTopByIdAsync(itemId, query, cancellation);
	public Task<IApiResponse<Response<Order[]>>> GetOrdersFromUserAsync(string slug, CancellationToken cancellation) =>
		_apiV2.GetOrdersFromUserAsync(slug, cancellation);
	public Task<IApiResponse<Response<Order[]>>> GetOrdersFromUserIdAsync(string userId, CancellationToken cancellation) =>
		_apiV2.GetOrdersFromUserIdAsync(userId, cancellation);
	public Task<IApiResponse<Response<User>>> GetUserAsync(string slug, CancellationToken cancellation) =>
		_apiV2.GetUserAsync(slug, cancellation);
	public Task<IApiResponse<Response<User>>> GetUserByIdAsync(string userId, CancellationToken cancellation) =>
		_apiV2.GetUserByIdAsync(userId, cancellation);
	public Task<IApiResponse<Response<Achievement[]>>> GetAchievementsAsync(CancellationToken cancellation) =>
		_apiV2.GetAchievementsAsync(cancellation);
	public Task<IApiResponse<Response<Achievement[]>>> GetUserAchievementsAsync(string slug, bool? featured, CancellationToken cancellation) =>
		_apiV2.GetUserAchievementsAsync(slug, featured, cancellation);
	public Task<IApiResponse<Response<Achievement[]>>> GetUserAchievementsByIdAsync(string userId, bool? featured, CancellationToken cancellation) =>
		_apiV2.GetUserAchievementsByIdAsync(userId, featured, cancellation);
	public Task<IApiResponse<Response<DashboardShowcase>>> GetDashboardShowcaseAsync(CancellationToken cancellation) =>
		_apiV2.GetDashboardShowcaseAsync(cancellation);

	/// <summary>
	/// 获取指定物品的统计数据（V1 端点，包装为 V2 统一响应格式）
	/// </summary>
	public async Task<Response<Statistic>> GetStatisticsAsync(string slug, CancellationToken cancellation = default)
	{
		var v1Result = await _apiV1.GetStatisticAsync(slug, cancellation);

		return v1Result.IsSuccessStatusCode && v1Result.Content != null
			? new Response<Statistic>("0.25.0", v1Result.Content, null)
			: new Response<Statistic>("0.25.0", null!, v1Result.Error?.ToString());
	}
}
