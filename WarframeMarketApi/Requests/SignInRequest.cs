namespace zms9110750.WarframeMarketApi.Requests;

/// <summary>
/// 登录请求体（仅第一方客户端可用，需 Firebase App Check）
/// </summary>
/// <param name="Email">用户邮箱，转小写，最长 128 字符</param>
/// <param name="Password">用户密码，最长 128 字符</param>
/// <param name="ClientId">已注册的第一方客户端 ID，6 ~ 64 字符</param>
/// <param name="DeviceId">用于绑定会话的设备唯一 ID，6 ~ 256 字符</param>
/// <param name="DeviceName">可读的设备名称，默认 "Unknown"，最长 128 字符</param>
internal record SignInRequest(
	string Email,
	string Password,
	string ClientId,
	string DeviceId,
	string? DeviceName = null
);
