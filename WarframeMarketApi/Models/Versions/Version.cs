using System.Globalization;

namespace zms9110750.WarframeMarketApi.Models.Versions;

/// <summary>
/// 服务器资源版本号
/// </summary>
/// <param name="Id">版本 ID</param>
/// <param name="Apps">各平台 App 版本信息</param>
/// <param name="Collections">各数据集合的版本哈希值</param>
/// <param name="UpdatedAt">最后更新时间（ISO 8601）</param>
public record Version(
	string Id,
	VersionApps Apps,
	VersionCollections Collections,
	string UpdatedAt
)
{
	/// <summary>最后更新时间（本地时间）</summary>
	public DateTime UpdatedAtLocal => DateTime.Parse(UpdatedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToLocalTime();
}
