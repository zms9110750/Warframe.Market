using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Axion.Extensions.Caching.Hybrid.Serialization.Http;

namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>
/// System.Text.Json 的 HttpResponseMessage 转换器（委托 Axion 专用序列化器，base64 往返）。
/// 注册到 FusionCache 的 SystemTextJson 序列化器后，FusionCache 序列化整个 entry 时
/// 嵌套的 HttpResponseMessage（RequestMessage.Properties 含 RuntimeType，通用 JSON 无法处理）
/// 会走此转换器，items 带硬盘缓存（落 Sqlite L2）不再失败。
/// </summary>
public sealed class HttpResponseMessageJsonConverter : JsonConverter<HttpResponseMessage>
{
    public override HttpResponseMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var bytes = Convert.FromBase64String(reader.GetString()!);
        return HttpResponseMessageHybridCacheSerializer.Instance.Deserialize(new ReadOnlySequence<byte>(bytes));
    }

    public override void Write(Utf8JsonWriter writer, HttpResponseMessage value, JsonSerializerOptions options)
    {
        var buffer = new ArrayBufferWriter<byte>();
        HttpResponseMessageHybridCacheSerializer.Instance.Serialize(value, buffer);
        writer.WriteStringValue(Convert.ToBase64String(buffer.WrittenSpan));
    }
}
