using Refit;
using zms9110750.WarframeMarketApi.Models.Statistics;

namespace zms9110750.WarframeMarketApi.Api;

/// <summary>
/// Warframe Market API - V1 端点（snake_case 序列化）
/// </summary>
public interface IWarframeMarketApiV1
{
	/// <summary>
	/// 获取指定物品的统计数据
	/// </summary>
	[Get("/v1/items/{slug}/statistics")]
	Task<IApiResponse<Statistic>> GetStatisticAsync(string slug, CancellationToken cancellation = default);
}
