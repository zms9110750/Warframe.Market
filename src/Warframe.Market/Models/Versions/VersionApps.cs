namespace zms9110750.WarframeMarketApi.Models.Versions;

/// <summary>
/// 各平台 App 版本信息
/// </summary>
/// <param name="Ios">最新 iOS 版本</param>
/// <param name="Android">最新 Android 版本</param>
/// <param name="MinIos">最低支持 iOS 版本</param>
/// <param name="MinAndroid">最低支持 Android 版本</param>
public record VersionApps(
    string Ios,
    string Android,
    string MinIos,
    string MinAndroid
);
