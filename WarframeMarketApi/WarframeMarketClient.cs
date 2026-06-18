using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.RateLimiting;
using Polly.RateLimiting;
using Polly.Retry;
using Refit;
using zms9110750.WarframeMarketApi.Api;
using zms9110750.WarframeMarketApi.Models;
using zms9110750.WarframeMarketApi.Models.Achievements;
using zms9110750.WarframeMarketApi.Models.Dashboard;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Liches;
using zms9110750.WarframeMarketApi.Models.Locations;
using zms9110750.WarframeMarketApi.Models.Missions;
using zms9110750.WarframeMarketApi.Models.Npcs;
using zms9110750.WarframeMarketApi.Models.Orders;
using zms9110750.WarframeMarketApi.Models.Rivens;
using zms9110750.WarframeMarketApi.Models.Sisters;
using zms9110750.WarframeMarketApi.Models.Statistics;
using zms9110750.WarframeMarketApi.Models.Users;
using Polly;
using Version = zms9110750.WarframeMarketApi.Models.Versions.Version;

namespace zms9110750.WarframeMarketApi;

/// <summary>
/// Warframe.Market API 客户端。
/// 实现所有公共端点，内置 Polly 弹性管道：
/// 429 重试（指数退避+抖动）、令牌桶限流（3/s）、限流拒绝无限重试。
/// </summary>
public class WarframeMarketClient : IWarframeMarketApiV2
{
	private static readonly JsonSerializerOptions V2Options = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
		Converters = { new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower) },
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	};

	private static readonly JsonSerializerOptions V1Options = new(V2Options)
	{
		PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
	};

	private static readonly ResiliencePipeline<HttpResponseMessage> Pipeline =
		new ResiliencePipelineBuilder<HttpResponseMessage>()
			.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
			{
				ShouldHandle = args => ValueTask.FromResult(
					args.Outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests),
				MaxRetryAttempts = 3,
				Delay = TimeSpan.FromSeconds(1),
				BackoffType = DelayBackoffType.Exponential,
				UseJitter = true,
			})
			.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
			{
				ShouldHandle = args => ValueTask.FromResult(
					args.Outcome.Exception is RateLimiterRejectedException),
				MaxRetryAttempts = int.MaxValue,
				Delay = TimeSpan.FromSeconds(1.5),
			})
			.AddRateLimiter(new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
			{
				PermitLimit = 3,
				SegmentsPerWindow = 1,
				Window = TimeSpan.FromSeconds(1),
			}))
			.Build();

	private readonly IWarframeMarketApiV2 _apiV2;
	private readonly HttpClient _httpClient;

	/// <summary>
	/// 使用默认配置创建客户端。
	/// 基址 https://api.warframe.market，内置 Polly 弹性管道（限流 3/s + 429 重试）。
	/// </summary>
	public WarframeMarketClient()
	{
		var innerHandler = new SocketsHttpHandler
		{
			PooledConnectionLifetime = TimeSpan.FromMinutes(5),
			EnableMultipleHttp2Connections = true,
		};

		var resilienceHandler = new ResilienceHandler(Pipeline, innerHandler);

		_httpClient = new HttpClient(resilienceHandler)
		{
			BaseAddress = new Uri("https://api.warframe.market")
		};
		_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("zms9110750.WarframeMarketApi/0.1.0");
		_httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
		_httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9");
		_httpClient.DefaultRequestHeaders.Add("Language", "zh-hans");
		_httpClient.DefaultRequestHeaders.Add("Platform", "pc");

		_apiV2 = RestService.For<IWarframeMarketApiV2>(_httpClient, new RefitSettings
		{
			ContentSerializer = new SystemTextJsonContentSerializer(V2Options)
		});
	}

	/// <summary>
	/// 使用自定义 HttpClient 创建客户端。
	/// 注意：自定义 HttpClient 不会自动附加 Polly 弹性管道，
	/// 请自行配置 BaseAddress、请求头、限流和重试。
	/// </summary>
	/// <param name="httpClient">已配置的 HttpClient（应设置 BaseAddress = https://api.warframe.market）</param>
	public WarframeMarketClient(HttpClient httpClient)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

		_apiV2 = RestService.For<IWarframeMarketApiV2>(_httpClient, new RefitSettings
		{
			ContentSerializer = new SystemTextJsonContentSerializer(V2Options)
		});
	}

	/// <inheritdoc/>
	public Task<IApiResponse<Response<Version>>> GetVersionsAsync(CancellationToken cancellation) =>
		_apiV2.GetVersionsAsync(cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<ItemShort[]>>> GetItemsAsync(CancellationToken cancellation) =>
		_apiV2.GetItemsAsync(cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<Item>>> GetItemAsync(string slug, CancellationToken cancellation) =>
		_apiV2.GetItemAsync(slug, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<Item>>> GetItemByIdAsync(string itemId, CancellationToken cancellation) =>
		_apiV2.GetItemByIdAsync(itemId, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<ItemSet>>> GetItemSetAsync(string slug, CancellationToken cancellation) =>
		_apiV2.GetItemSetAsync(slug, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<ItemSet>>> GetItemSetByIdAsync(string itemId, CancellationToken cancellation) =>
		_apiV2.GetItemSetByIdAsync(itemId, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<Riven[]>>> GetRivenWeaponsAsync(CancellationToken cancellation) =>
		_apiV2.GetRivenWeaponsAsync(cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<Riven>>> GetRivenWeaponAsync(string slug, CancellationToken cancellation) =>
		_apiV2.GetRivenWeaponAsync(slug, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<RivenAttribute[]>>> GetRivenAttributesAsync(CancellationToken cancellation) =>
		_apiV2.GetRivenAttributesAsync(cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<LichWeapon[]>>> GetLichWeaponsAsync(CancellationToken cancellation) =>
		_apiV2.GetLichWeaponsAsync(cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<LichWeapon>>> GetLichWeaponAsync(string slug, CancellationToken cancellation) =>
		_apiV2.GetLichWeaponAsync(slug, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<LichEphemera[]>>> GetLichEphemerasAsync(CancellationToken cancellation) =>
		_apiV2.GetLichEphemerasAsync(cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<LichQuirk[]>>> GetLichQuirksAsync(CancellationToken cancellation) =>
		_apiV2.GetLichQuirksAsync(cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<SisterWeapon[]>>> GetSisterWeaponsAsync(CancellationToken cancellation) =>
		_apiV2.GetSisterWeaponsAsync(cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<SisterWeapon>>> GetSisterWeaponAsync(string slug, CancellationToken cancellation) =>
		_apiV2.GetSisterWeaponAsync(slug, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<SisterEphemera[]>>> GetSisterEphemerasAsync(CancellationToken cancellation) =>
		_apiV2.GetSisterEphemerasAsync(cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<SisterQuirk[]>>> GetSisterQuirksAsync(CancellationToken cancellation) =>
		_apiV2.GetSisterQuirksAsync(cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<Location[]>>> GetLocationsAsync(CancellationToken cancellation) =>
		_apiV2.GetLocationsAsync(cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<Npc[]>>> GetNpcsAsync(CancellationToken cancellation) =>
		_apiV2.GetNpcsAsync(cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<Mission[]>>> GetMissionsAsync(CancellationToken cancellation) =>
		_apiV2.GetMissionsAsync(cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<Order[]>>> GetOrdersRecentAsync(CancellationToken cancellation) =>
		_apiV2.GetOrdersRecentAsync(cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<Order[]>>> GetOrdersItemAsync(string slug, CancellationToken cancellation) =>
		_apiV2.GetOrdersItemAsync(slug, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<Order[]>>> GetOrdersItemByIdAsync(string itemId, CancellationToken cancellation) =>
		_apiV2.GetOrdersItemByIdAsync(itemId, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<OrderTop>>> GetOrdersItemTopAsync(string slug, OrderTopQueryParameter? query, CancellationToken cancellation) =>
		_apiV2.GetOrdersItemTopAsync(slug, query, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<OrderTop>>> GetOrdersItemTopByIdAsync(string itemId, OrderTopQueryParameter? query, CancellationToken cancellation) =>
		_apiV2.GetOrdersItemTopByIdAsync(itemId, query, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<Order[]>>> GetOrdersFromUserAsync(string slug, CancellationToken cancellation) =>
		_apiV2.GetOrdersFromUserAsync(slug, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<Order[]>>> GetOrdersFromUserIdAsync(string userId, CancellationToken cancellation) =>
		_apiV2.GetOrdersFromUserIdAsync(userId, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<User>>> GetUserAsync(string slug, CancellationToken cancellation) =>
		_apiV2.GetUserAsync(slug, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<User>>> GetUserByIdAsync(string userId, CancellationToken cancellation) =>
		_apiV2.GetUserByIdAsync(userId, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<Achievement[]>>> GetAchievementsAsync(CancellationToken cancellation) =>
		_apiV2.GetAchievementsAsync(cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<Achievement[]>>> GetUserAchievementsAsync(string slug, bool? featured, CancellationToken cancellation) =>
		_apiV2.GetUserAchievementsAsync(slug, featured, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<Achievement[]>>> GetUserAchievementsByIdAsync(string userId, bool? featured, CancellationToken cancellation) =>
		_apiV2.GetUserAchievementsByIdAsync(userId, featured, cancellation);

	/// <inheritdoc/>
	public Task<IApiResponse<Response<DashboardShowcase>>> GetDashboardShowcaseAsync(CancellationToken cancellation) =>
		_apiV2.GetDashboardShowcaseAsync(cancellation);

	/// <summary>
	/// 获取指定物品的统计数据（V1 端点，内部自动反序列化并包装为 V2 统一响应格式）
	/// </summary>
	public async Task<Response<Statistic>> GetStatisticsAsync(string slug, CancellationToken cancellation = default)
	{
		try
		{
			var json = await _httpClient.GetStringAsync($"/v1/items/{slug}/statistics", cancellation);
			var statistic = JsonSerializer.Deserialize<Statistic>(json, V1Options);
			return new Response<Statistic>("0.25.0", statistic!, null);
		}
		catch (HttpRequestException ex)
		{
			return new Response<Statistic>("0.25.0", null!, ex.Message);
		}
	}
}

/// <summary>
/// 使用 Polly 弹性管道包装 HttpMessageHandler 的委托处理程序
/// </summary>
internal class ResilienceHandler : DelegatingHandler
{
	private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

	public ResilienceHandler(ResiliencePipeline<HttpResponseMessage> pipeline, HttpMessageHandler inner)
		: base(inner)
	{
		_pipeline = pipeline;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
	{
		return await _pipeline.ExecuteAsync(async cancel =>
		{
			var clone = await CloneHttpRequestMessageAsync(request);
			return await base.SendAsync(clone, cancel);
		}, ct);
	}

	private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
	{
		var clone = new HttpRequestMessage(request.Method, request.RequestUri);

		if (request.Content != null)
		{
			var body = await request.Content.ReadAsByteArrayAsync();
			clone.Content = new ByteArrayContent(body);
			foreach (var header in request.Content.Headers)
			{
				clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}
		}

		foreach (var header in request.Headers)
		{
			clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
		}

		foreach (var option in request.Options)
		{
			clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
		}

		return clone;
	}
}
