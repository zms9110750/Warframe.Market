using Refit;
using zms9110750.WarframeMarketApi.Requests;

namespace zms9110750.WarframeMarketApi.Api;

/// <summary>
/// Warframe Market API - 认证端点
/// </summary>
internal interface IWarframeMarketAuthApi
{
	/// <summary>
	/// 使用第一方客户端登录并创建会话
	/// </summary>
	[Post("/auth/signin")]
	Task<IApiResponse<SignInResponse>> SignInAsync([Body] SignInRequest request, CancellationToken cancellation = default);

	/// <summary>
	/// 使用第一方客户端注册新用户
	/// </summary>
	[Post("/auth/signup")]
	Task<IApiResponse<SignInResponse>> SignUpAsync([Body] SignUpRequest request, CancellationToken cancellation = default);

	/// <summary>
	/// 使用 refresh token 刷新会话
	/// </summary>
	[Post("/auth/refresh")]
	Task<IApiResponse<SignInResponse>> RefreshAsync([Body] RefreshRequest request, CancellationToken cancellation = default);

	/// <summary>
	/// 终止当前会话
	/// </summary>
	[Post("/auth/signout")]
	Task<IApiResponse> SignOutAsync(CancellationToken cancellation = default);
}

/// <summary>
/// 认证响应
/// </summary>
/// <param name="AccessToken">JWT 访问令牌</param>
/// <param name="RefreshToken">刷新令牌</param>
/// <param name="TokenType">令牌类型（Bearer）</param>
/// <param name="ExpiresIn">访问令牌有效期（秒）</param>
internal record SignInResponse(
	string AccessToken,
	string RefreshToken,
	string TokenType,
	int ExpiresIn
);
