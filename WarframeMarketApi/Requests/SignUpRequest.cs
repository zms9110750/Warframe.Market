namespace zms9110750.WarframeMarketApi.Requests;

/// <summary>
/// 注册请求体（仅第一方客户端可用，需 Firebase App Check）
/// </summary>
/// <param name="Email">用户邮箱，转小写，最长 128 字符</param>
/// <param name="Password">用户密码，6 ~ 128 字符</param>
/// <param name="PasswordConfirmation">确认密码，须与 Password 一致</param>
/// <param name="ClientId">已注册的第一方客户端 ID，6 ~ 64 字符</param>
/// <param name="DeviceId">用于绑定会话的设备唯一 ID，6 ~ 256 字符</param>
/// <param name="Platform">用户起始平台</param>
/// <param name="Locale">偏好通信语言</param>
/// <param name="DeviceName">可读的设备名称，默认 "Unknown"，最长 128 字符</param>
public record SignUpRequest(
	string Email,
	string Password,
	string PasswordConfirmation,
	string ClientId,
	string DeviceId,
	string Platform,
	string Locale,
	string? DeviceName = null
);
