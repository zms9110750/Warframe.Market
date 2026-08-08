using System.Buffers;
using System.Net;
using System.Text;
using Axion.Extensions.Polly.Caching.Hybrid;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>
/// 验证 README 声称的 "Polly + FusionCache 给 HTTP 加缓存" 是否真实有效：
/// AddResilienceHandler + AddCaching（Axion.Extensions.Http.Resilience.Caching.Hybrid）
/// 同一 URL 第二次请求应命中缓存，底层 handler 不再被调用。
/// </summary>
public class PollyHttpCacheTests
{
    private sealed class CountingHandler : HttpMessageHandler
    {
        public int Calls;
        private readonly string? _body;
        private readonly HttpStatusCode _status;
        private readonly bool _throw;

        public CountingHandler(string? body = "{}", HttpStatusCode status = HttpStatusCode.OK, bool throwEx = false)
        {
            _body = body;
            _status = status;
            _throw = throwEx;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Calls);
            if (_throw)
            {
                throw new HttpRequestException("boom");
            }
            var response = new HttpResponseMessage(_status);
            if (_body != null)
            {
                response.Content = new StringContent(_body, Encoding.UTF8, "application/json");
            }
            response.RequestMessage = request; // 模拟真实链路：响应携带 RequestMessage
            return Task.FromResult(response);
        }
    }

    [Fact]
    public async Task Second_request_with_same_url_hits_http_cache()
    {
        var services = new ServiceCollection();
        services.AddSqliteCache("httpcache-test.db")
            .AddFusionCache()
            .WithRegisteredDistributedCache()
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .AsHybridCache();

        var handler = new CountingHandler("{\"apiVersion\":\"0.25.0\",\"data\":[],\"error\":null}");

        services.AddHttpClient("t", c => c.BaseAddress = new Uri("https://api.warframe.market"))
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddResilienceHandler("h", (pipeline, ctx) => {
                pipeline.AddCaching(new HttpCachingStrategyOptions {
                    HybridCache = ctx.ServiceProvider.GetRequiredService<HybridCache>()
                });
            });

        await using var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("t");

        var r1 = await client.GetAsync("/v2/items");
        var r2 = await client.GetAsync("/v2/items");

        Assert.Equal(HttpStatusCode.OK, r1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, r2.StatusCode);
        Assert.Equal(1, handler.Calls); // 第二次命中缓存，handler 未被再次调用
        Assert.Equal(await r1.Content.ReadAsStringAsync(), await r2.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Different_urls_do_not_share_cache()
    {
        var services = new ServiceCollection();
        services.AddSqliteCache("httpcache-test.db")
            .AddFusionCache()
            .WithRegisteredDistributedCache()
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .AsHybridCache();

        var handler = new CountingHandler("{\"data\":\"x\"}");

        services.AddHttpClient("t", c => c.BaseAddress = new Uri("https://api.warframe.market"))
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddResilienceHandler("h", (pipeline, ctx) => {
                pipeline.AddCaching(new HttpCachingStrategyOptions {
                    HybridCache = ctx.ServiceProvider.GetRequiredService<HybridCache>()
                });
            });

        await using var sp = services.BuildServiceProvider();
        var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("t");

        await client.GetAsync("/v2/items");
        await client.GetAsync("/v2/item/prisma-kronen");

        Assert.Equal(2, handler.Calls); // 不同 URL 不共享缓存
    }

    [Fact]
    public async Task Default_provider_shares_cache_across_query_strings()
    {
        // 包默认 CacheKeyProvider 不含 query：带不同查询参数的请求会串缓存（已知行为）
        var services = new ServiceCollection();
        services.AddSqliteCache("httpcache-test.db")
            .AddFusionCache()
            .WithRegisteredDistributedCache()
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .AsHybridCache();

        var handler = new CountingHandler("{\"data\":\"x\"}");
        services.AddHttpClient("t", c => c.BaseAddress = new Uri("https://api.warframe.market"))
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddResilienceHandler("h", (pipeline, ctx) => {
                pipeline.AddCaching(new HttpCachingStrategyOptions {
                    HybridCache = ctx.ServiceProvider.GetRequiredService<HybridCache>()
                });
            });

        await using var sp = services.BuildServiceProvider();
        var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("t");

        await client.GetAsync("/v2/orders/item/prisma-kronen/top?rankLt=5");
        await client.GetAsync("/v2/orders/item/prisma-kronen/top?rankLt=3");

        Assert.Equal(1, handler.Calls); // 串缓存：第二次命中，query 被忽略
    }

    [Fact]
    public async Task Custom_provider_with_query_isolates_query_strings()
    {
        // 自定义 CacheKeyProvider 拼上 query：不同查询参数不再串缓存
        var services = new ServiceCollection();
        services.AddSqliteCache("httpcache-test.db")
            .AddFusionCache()
            .WithRegisteredDistributedCache()
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .AsHybridCache();

        var handler = new CountingHandler("{\"data\":\"x\"}");
        services.AddHttpClient("t", c => c.BaseAddress = new Uri("https://api.warframe.market"))
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddResilienceHandler("h", (pipeline, ctx) => {
                pipeline.AddCaching(new HttpCachingStrategyOptions {
                    HybridCache = ctx.ServiceProvider.GetRequiredService<HybridCache>(),
                    CacheKeyProvider = QueryAwareCacheKeyProvider
                });
            });

        await using var sp = services.BuildServiceProvider();
        var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("t");

        await client.GetAsync("/v2/orders/item/prisma-kronen/top?rankLt=5");
        await client.GetAsync("/v2/orders/item/prisma-kronen/top?rankLt=3");

        Assert.Equal(2, handler.Calls); // 隔离成功
    }

    private static ValueTask<string> QueryAwareCacheKeyProvider(Polly.ResilienceContext context)
    {
        var message = context.GetRequestMessage() ?? throw new InvalidOperationException();
        var uri = message.RequestUri ?? throw new InvalidOperationException();
        var key = $"{message.Method.Method.ToLowerInvariant()}/{uri.Scheme}/{uri.IdnHost}{uri.LocalPath}{uri.Query}";
        return new ValueTask<string>(key);
    }
    [Fact]
    public async Task HttpResponseMessage_generic_json_serializer_fails_but_axion_serializer_roundtrips()
    {
        var msg = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"a\":1}", Encoding.UTF8, "application/json")
        };
        msg.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.warframe.market/v2/items");
        msg.RequestMessage.Properties["x"] = typeof(string); // RuntimeType：通用序列化器的雷

        // 复现：通用 JSON 序列化器无法处理 HttpResponseMessage
        var json = new FusionCacheSystemTextJsonSerializer();
        Assert.Throws<NotSupportedException>(() => json.Serialize(msg));

        // 修复：Axion 专用序列化器往返成功
        var axion = Axion.Extensions.Caching.Hybrid.Serialization.Http.HttpResponseMessageHybridCacheSerializer.Instance;
        var writer = new ArrayBufferWriter<byte>();
        axion.Serialize(msg, writer);
        var restored = axion.Deserialize(new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray()));

        Assert.Equal(HttpStatusCode.OK, restored.StatusCode);
        Assert.Equal("{\"a\":1}", await restored.Content.ReadAsStringAsync());
    }
    [Fact]
    public async Task Nested_httpresponsemessage_serializes_via_converter()
    {
        // 模拟 FusionCache 分布式层：序列化的是包着值的 entry（嵌套对象），不是根值
        var msg = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"a\":1}", Encoding.UTF8, "application/json")
        };
        msg.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.warframe.market/v2/items");
        msg.RequestMessage.Properties["x"] = typeof(string); // RuntimeType：通用序列化器的雷

        // 复现：无 converter 时嵌套对象（entry 结构）序列化失败
        var plain = new System.Text.Json.JsonSerializerOptions();
        Assert.Throws<NotSupportedException>(() => System.Text.Json.JsonSerializer.Serialize(new Box(msg), plain));

        // 修复：converter 挂在 System.Text.Json options 上，嵌套的 HttpResponseMessage 也能拦截
        var opts = new System.Text.Json.JsonSerializerOptions { Converters = { new HttpResponseMessageConverterStub() } };
        var json = System.Text.Json.JsonSerializer.Serialize(new Box(msg), opts);
        var restored = System.Text.Json.JsonSerializer.Deserialize<Box>(json, opts);

        Assert.Equal(HttpStatusCode.OK, restored!.Value.StatusCode);
        Assert.Equal("{\"a\":1}", await restored.Value.Content.ReadAsStringAsync());
    }

    private sealed record Box(HttpResponseMessage Value);

    /// <summary>等价于 GUI 的 HttpResponseMessageJsonConverter（测试无法引用 GUI 项目，内联复刻）</summary>
    private sealed class HttpResponseMessageConverterStub : System.Text.Json.Serialization.JsonConverter<HttpResponseMessage>
    {
        public override HttpResponseMessage Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            var bytes = Convert.FromBase64String(reader.GetString()!);
            return Axion.Extensions.Caching.Hybrid.Serialization.Http.HttpResponseMessageHybridCacheSerializer.Instance
                .Deserialize(new System.Buffers.ReadOnlySequence<byte>(bytes));
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, HttpResponseMessage value, System.Text.Json.JsonSerializerOptions options)
        {
            var buffer = new System.Buffers.ArrayBufferWriter<byte>();
            Axion.Extensions.Caching.Hybrid.Serialization.Http.HttpResponseMessageHybridCacheSerializer.Instance
                .Serialize(value, buffer);
            writer.WriteStringValue(Convert.ToBase64String(buffer.WrittenSpan));
        }
    }
}