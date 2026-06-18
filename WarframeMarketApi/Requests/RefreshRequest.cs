namespace zms9110750.WarframeMarketApi.Requests;

/// <summary>
/// 刷新会话请求体
/// </summary>
/// <param name="GrantType">必须为 "refresh_token"</param>
/// <param name="ClientId">已注册的客户端 ID，6 ~ 64 字符</param>
/// <param name="DeviceId">绑定到会话的设备 ID，6 ~ 256 字符</param>
/// <param name="RefreshToken">现有会话的刷新令牌，6 ~ 256 字符</param>
internal record RefreshRequest(
	string GrantType,
	string ClientId,
	string DeviceId,
	string RefreshToken
);
