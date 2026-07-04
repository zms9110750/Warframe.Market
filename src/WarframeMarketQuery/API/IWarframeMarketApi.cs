using Refit;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WarframeMarketQuery.Model;
using WarframeMarketQuery.Model.Items;
using WarframeMarketQuery.Model.Orders;
using WarframeMarketQuery.Model.Users;
using Version = WarframeMarketQuery.Model.Versions.Version;
namespace WarframeMarketQuery.API;

/// <summary>
/// Warframe Market API - v2 endpoints (使用 V2 序列化设置)
/// 请在注册 Refit 客户端时为此接口使用 Response.V2options 的 <see cref="RefitSettings.ContentSerializer"/>。
/// </summary>
public interface IWarframeMarketApi
{
    /// <summary>
    /// 该端点获取服务器资源的当前版本号
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Get("/v2/versions")]
    Task<IApiResponse<Response<Version>>> GetVersionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有可交易物品的列表
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Get("/v2/items")]
    Task<IApiResponse<Response<ItemShort[]>>> GetItemsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取关于某一件特定物品的完整信息
    /// </summary>
    /// <param name="slug"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Get("/v2/item/{slug}")]
    Task<IApiResponse<Response<Item>>> GetItemAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检索合集信息。在 WFM 中，物品可以是独立的，也可以是集合的一部分。一套是一组相关物品，通常被交易在一起。
    /// </summary>
    /// <param name="slug"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Get("/v2/item/{slug}/set")]
    Task<IApiResponse<Response<ItemSet>>> GetItemSetAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取最新的订单。最多 500，过去 4 小时内。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Get("/v2/orders/recent")]
    Task<IApiResponse<Response<Order[]>>> GetOrdersRecentAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取过去7天内在线用户的所有订单清单。
    /// </summary>
    /// <param name="slug"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Get("/v2/orders/item/{slug}")]
    Task<IApiResponse<Response<Order[]>>> GetOrdersItemAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// 该终端设计用于获取特定商品的前5个买入和前5个卖出订单，仅限在线用户。
    /// </summary>
    /// <param name="slug"></param>
    /// <param name="query"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Get("/v2/orders/item/{slug}/top")]
    Task<IApiResponse<Response<OrderTop>>> GetOrdersItemTopAsync(string slug, [Query] OrderTopQueryParameter? query = null, CancellationToken cancellation = default);

    /// <summary>
    /// 从指定用户那里获取公开订单
    /// </summary>
    /// <param name="slug"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Get("/v2/orders/user/{slug}")]
    Task<IApiResponse<Response<Order[]>>> GetOrdersFromUserAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取特定用户的信息
    /// </summary>
    /// <param name="slug"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Get("/v2/user/{slug}")]
    Task<IApiResponse<Response<User>>> GetUserAsync(string slug, CancellationToken cancellationToken = default);

    public static JsonSerializerOptions V2options { get; } =
        new JsonSerializerOptions
        {
            // 使用源生成器的类型信息解析器
            TypeInfoResolver = JsonTypeInfoResolver.Combine(
                SourceGenerationContext.Default.Options.TypeInfoResolver,  // 优先使用源生成
                new DefaultJsonTypeInfoResolver()// 回退到反射 	
            ),
            // 自定义配置
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  // 蛇形命名
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower) },
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
}
