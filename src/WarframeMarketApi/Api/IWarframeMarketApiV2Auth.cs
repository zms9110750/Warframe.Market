using Refit;
using zms9110750.WarframeMarketApi.Models;
using zms9110750.WarframeMarketApi.Models.Orders;
using zms9110750.WarframeMarketApi.Models.Users;
using zms9110750.WarframeMarketApi.Requests;

namespace zms9110750.WarframeMarketApi.Api;

/// <summary>
/// Warframe Market API - V2 需认证端点
/// </summary>
internal interface IWarframeMarketApiV2Auth
{
	// ===== Orders =====

	/// <summary>
	/// 获取当前认证用户的所有订单
	/// </summary>
	[Get("/v2/orders/my")]
	Task<IApiResponse<Response<Order[]>>> GetMyOrdersAsync(CancellationToken cancellation = default);

	/// <summary>
	/// 按 ID 获取单个订单
	/// </summary>
	[Get("/v2/order/{id}")]
	Task<IApiResponse<Response<Order>>> GetOrderAsync(string id, CancellationToken cancellation = default);

	/// <summary>
	/// 创建新订单
	/// </summary>
	[Post("/v2/order")]
	Task<IApiResponse<Response<Order>>> CreateOrderAsync([Body] CreateOrderRequest request, CancellationToken cancellation = default);

	/// <summary>
	/// 更新现有订单
	/// </summary>
	[Patch("/v2/order/{id}")]
	Task<IApiResponse<Response<Order>>> PatchOrderAsync(string id, [Body] PatchOrderRequest request, CancellationToken cancellation = default);

	/// <summary>
	/// 删除订单
	/// </summary>
	[Delete("/v2/order/{id}")]
	Task<IApiResponse<Response<Order>>> DeleteOrderAsync(string id, CancellationToken cancellation = default);

	/// <summary>
	/// 关闭部分或全部订单
	/// </summary>
	[Post("/v2/order/{id}/close")]
	Task<IApiResponse<Response<Transaction>>> CloseOrderAsync(string id, [Body] CloseOrderRequest request, CancellationToken cancellation = default);

	/// <summary>
	/// 更新虚拟订单分组的可见性
	/// </summary>
	[Patch("/v2/orders/group/{id}")]
	Task<IApiResponse<Response<PatchOrdersGroupResponse>>> PatchOrdersGroupAsync(string id, [Body] PatchOrdersGroupRequest request, CancellationToken cancellation = default);

	// ===== Users =====

	/// <summary>
	/// 获取当前认证用户的信息
	/// </summary>
	[Get("/v2/me")]
	Task<IApiResponse<Response<UserPrivate>>> GetMeAsync(CancellationToken cancellation = default);

	/// <summary>
	/// 更新当前用户的资料偏好
	/// </summary>
	[Patch("/v2/me")]
	Task<IApiResponse<Response<UserPrivate>>> PatchMeAsync([Body] UpdateMeRequest request, CancellationToken cancellation = default);

	/// <summary>
	/// 上传头像
	/// </summary>
	[Post("/v2/me/avatar")]
	Task<IApiResponse<Response<UserPrivate>>> UploadAvatarAsync([Body(BodySerializationMethod.UrlEncoded)] StreamPart avatar, CancellationToken cancellation = default);

	/// <summary>
	/// 上传个人背景图（需要 silver 及以上订阅）
	/// </summary>
	[Post("/v2/me/background")]
	Task<IApiResponse<Response<UserPrivate>>> UploadBackgroundAsync([Body(BodySerializationMethod.UrlEncoded)] StreamPart background, CancellationToken cancellation = default);
}

/// <summary>
/// 更新虚拟订单分组可见性的响应
/// </summary>
/// <param name="Updated">更新影响的订单数量</param>
internal record PatchOrdersGroupResponse(int Updated);
