using Refit;
using System.Text.Json;
using WarframeMarketQuery.Model.Statistics;

namespace WarframeMarketQuery.API;

/// <summary>
/// Warframe Market API - v1 endpoints（使用 V1 序列化设置）
/// 与 <see cref="IWarframeMarketApi"/> 分开定义，注册时请使用不同的 RefitSettings（ContentSerializer 使用 Response.V1options），避免混杂设置。
/// </summary>
public interface IWarframeMarketApiV1
{
    /// <summary>
    /// 获取特定物品的统计数据
    /// </summary>
    /// <param name="slug"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Get("/v1/items/{slug}/statistics")]
    Task<IApiResponse<Statistic>> GetStatisticAsync(string slug, CancellationToken cancellationToken = default);
    public static JsonSerializerOptions V1options { get; } = new JsonSerializerOptions(IWarframeMarketApi.V2options)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}