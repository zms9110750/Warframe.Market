using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.RateLimiting;
using Polly;
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
using zms9110750.WarframeMarketApi.Models.Versions;

namespace zms9110750.WarframeMarketApi;

/// <summary>
/// Warframe.Market API 客户端。
/// 实现所有公共端点，内置 Polly 弹性管道：
/// 429 重试（指数退避+抖动）、令牌桶限流（3/s）、限流拒绝无限重试。
/// </summary>
public class WarframeMarketClient : IWarframeMarketApiV2
{
    private static readonly JsonSerializerOptions V2Options = new(JsonSerializerDefaults.Web) {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower) },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private static readonly JsonSerializerOptions V1Options = new(V2Options) {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private static readonly ResiliencePipeline<HttpResponseMessage> Pipeline =
        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage> {
                ShouldHandle = args => ValueTask.FromResult(
                    args.Outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
            })
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage> {
                ShouldHandle = args => ValueTask.FromResult(
                    args.Outcome.Exception is RateLimiterRejectedException),
                MaxRetryAttempts = int.MaxValue,
                Delay = TimeSpan.FromSeconds(1.5),
            })
            .AddRateLimiter(new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions {
                PermitLimit = 3,
                SegmentsPerWindow = 1,
                Window = TimeSpan.FromSeconds(1),
                QueueLimit = 6
            }))
            .Build();

    private readonly IWarframeMarketApiV2 _apiV2;
    private readonly HttpClient _httpClient;
    private Language _language;
    private Platform _platform;
    private bool _crossplay;

    /// <summary>
    /// 请求返回内容的语言。修改后自动更新请求头 <c>Language</c>。
    /// 默认 zh-hans，支持 en/ko/ru/de/fr/pt/zh-hans/zh-hant/es/it/pl/uk/tr/ja。
    /// </summary>
    public Language Language
    {
        get => _language;
        set
        {
            _language = value;
            _httpClient.DefaultRequestHeaders.Remove("Language");
            _httpClient.DefaultRequestHeaders.Add("Language",
                JsonNamingPolicy.KebabCaseLower.ConvertName(value.ToString()));
        }
    }

    /// <summary>
    /// 筛选结果的游戏平台。修改后自动更新请求头 <c>Platform</c>。
    /// 默认 pc，支持 pc/ps4/xbox/switch/mobile。
    /// </summary>
    public Platform Platform
    {
        get => _platform;
        set
        {
            _platform = value;
            _httpClient.DefaultRequestHeaders.Remove("Platform");
            _httpClient.DefaultRequestHeaders.Add("Platform",
                JsonNamingPolicy.KebabCaseLower.ConvertName(value.ToString()));
        }
    }

    /// <summary>
    /// 是否启用跨平台交易。修改后自动更新请求头 <c>Crossplay</c>。
    /// 默认 false。
    /// </summary>
    public bool Crossplay
    {
        get => _crossplay;
        set
        {
            _crossplay = value;
            _httpClient.DefaultRequestHeaders.Remove("Crossplay");
            _httpClient.DefaultRequestHeaders.Add("Crossplay", value.ToString().ToLowerInvariant());
        }
    }

    /// <summary>
    /// 使用默认配置创建客户端。
    /// 基址 https://api.warframe.market，内置 Polly 弹性管道（限流 3/s + 429 重试）。
    /// User-Agent 自动取调用方程序集名和版本。
    /// </summary>
    public WarframeMarketClient()
    {
        var innerHandler = new SocketsHttpHandler {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            EnableMultipleHttp2Connections = true,
        };

        var resilienceHandler = new ResilienceHandler(Pipeline, innerHandler);

        _httpClient = new HttpClient(resilienceHandler) {
            BaseAddress = new Uri("https://api.warframe.market")
        };
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9");

        var asm = Assembly.GetEntryAssembly();
        if (asm != null)
        {
            var name = asm.GetName().Name;
            var ver = asm.GetName().Version;
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                $"{name}/{ver?.ToString() ?? "null"}");
        }

        Language = Language.ZhHans;
        Platform = Platform.PC;
        Crossplay = false;

        _apiV2 = RestService.For<IWarframeMarketApiV2>(_httpClient, new RefitSettings {
            ContentSerializer = new SystemTextJsonContentSerializer(V2Options),
            UrlParameterKeyFormatter = new CamelCaseUrlParameterKeyFormatter()
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

        var asm = Assembly.GetEntryAssembly();
        if (asm != null)
        {
            var name = asm.GetName().Name;
            var ver = asm.GetName().Version;
            if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                    $"{name}/{ver?.ToString() ?? "null"}");
            }
        }

        _apiV2 = RestService.For<IWarframeMarketApiV2>(_httpClient, new RefitSettings {
            ContentSerializer = new SystemTextJsonContentSerializer(V2Options),
            UrlParameterKeyFormatter = new CamelCaseUrlParameterKeyFormatter()
        });
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<ServerVersion>>> GetVersionsAsync(CancellationToken cancellation = default)
    {
        return _apiV2.GetVersionsAsync(cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<ItemShort[]>>> GetItemsAsync(CancellationToken cancellation = default)
    {
        return _apiV2.GetItemsAsync(cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<Item>>> GetItemAsync(string slugOrItemId, CancellationToken cancellation = default)
    {
        return _apiV2.GetItemAsync(slugOrItemId, cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<ItemSet>>> GetItemSetAsync(string slugOrItemId, CancellationToken cancellation = default)
    {
        return _apiV2.GetItemSetAsync(slugOrItemId, cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<Riven[]>>> GetRivenWeaponsAsync(CancellationToken cancellation = default)
    {
        return _apiV2.GetRivenWeaponsAsync(cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<Riven>>> GetRivenWeaponAsync(string slug, CancellationToken cancellation = default)
    {
        return _apiV2.GetRivenWeaponAsync(slug, cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<RivenAttribute[]>>> GetRivenAttributesAsync(CancellationToken cancellation = default)
    {
        return _apiV2.GetRivenAttributesAsync(cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<LichWeapon[]>>> GetLichWeaponsAsync(CancellationToken cancellation = default)
    {
        return _apiV2.GetLichWeaponsAsync(cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<LichWeapon>>> GetLichWeaponAsync(string slug, CancellationToken cancellation = default)
    {
        return _apiV2.GetLichWeaponAsync(slug, cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<LichEphemera[]>>> GetLichEphemerasAsync(CancellationToken cancellation = default)
    {
        return _apiV2.GetLichEphemerasAsync(cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<LichQuirk[]>>> GetLichQuirksAsync(CancellationToken cancellation = default)
    {
        return _apiV2.GetLichQuirksAsync(cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<SisterWeapon[]>>> GetSisterWeaponsAsync(CancellationToken cancellation = default)
    {
        return _apiV2.GetSisterWeaponsAsync(cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<SisterWeapon>>> GetSisterWeaponAsync(string slug, CancellationToken cancellation = default)
    {
        return _apiV2.GetSisterWeaponAsync(slug, cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<SisterEphemera[]>>> GetSisterEphemerasAsync(CancellationToken cancellation = default)
    {
        return _apiV2.GetSisterEphemerasAsync(cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<SisterQuirk[]>>> GetSisterQuirksAsync(CancellationToken cancellation = default)
    {
        return _apiV2.GetSisterQuirksAsync(cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<Location[]>>> GetLocationsAsync(CancellationToken cancellation = default)
    {
        return _apiV2.GetLocationsAsync(cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<Npc[]>>> GetNpcsAsync(CancellationToken cancellation = default)
    {
        return _apiV2.GetNpcsAsync(cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<Mission[]>>> GetMissionsAsync(CancellationToken cancellation = default)
    {
        return _apiV2.GetMissionsAsync(cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<Order[]>>> GetOrdersRecentAsync(CancellationToken cancellation = default)
    {
        return _apiV2.GetOrdersRecentAsync(cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<Order[]>>> GetOrdersItemAsync(string slugOrItemId, CancellationToken cancellation = default)
    {
        return _apiV2.GetOrdersItemAsync(slugOrItemId, cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<OrderTop>>> GetOrdersItemTopAsync(string slugOrItemId, OrderTopQueryParameter? query, CancellationToken cancellation = default)
    {
        return _apiV2.GetOrdersItemTopAsync(slugOrItemId, query, cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<Order[]>>> GetOrdersFromUserAsync(string slugOrUserId, CancellationToken cancellation = default)
    {
        return _apiV2.GetOrdersFromUserAsync(slugOrUserId, cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<User>>> GetUserAsync(string slugOrUserId, CancellationToken cancellation = default)
    {
        return _apiV2.GetUserAsync(slugOrUserId, cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<Achievement[]>>> GetAchievementsAsync(CancellationToken cancellation = default)
    {
        return _apiV2.GetAchievementsAsync(cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<Achievement[]>>> GetUserAchievementsAsync(string slugOrUserId, bool? featured, CancellationToken cancellation = default)
    {
        return _apiV2.GetUserAchievementsAsync(slugOrUserId, featured, cancellation);
    }

    /// <inheritdoc/>
    public Task<IApiResponse<Response<DashboardShowcase>>> GetDashboardShowcaseAsync(CancellationToken cancellation = default)
    {
        return _apiV2.GetDashboardShowcaseAsync(cancellation);
    }

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
        return await _pipeline.ExecuteAsync(async cancel => {
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

        return clone;
    }
}


