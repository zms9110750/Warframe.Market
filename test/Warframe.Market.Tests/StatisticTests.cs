using System.Text.Json;
using zms9110750.WarframeMarketApi.Models.Statistics;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>
/// V1 统计反序列化 + 参考价计算实证（真实备份数据）
/// </summary>
public class StatisticTests
{
    private static readonly JsonSerializerOptions V1Options = new(JsonSerializerDefaults.Web) {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    [Fact]
    public void V1_statistics_deserializes_and_reference_price_is_not_null()
    {
        var json = File.ReadAllText(Data.File("statistics", "secura_dual_cestra.json"));
        var stat = JsonSerializer.Deserialize<Statistic>(json, V1Options);

        Assert.NotNull(stat);
        Assert.NotNull(stat!.Payload);
        Assert.NotNull(stat.Payload.StatisticsClosed);
        Assert.NotNull(stat.Payload.StatisticsClosed.Day90);
        Assert.NotEmpty(stat.Payload.StatisticsClosed.Day90);

        // 参考价（库层 StatisticPrice）：普通物品统计无 mod_rank（全 null → 命中 R0 过滤）
        Assert.NotNull(stat.GetReferencePrice());
        Assert.True(stat.GetReferencePrice() > 0);
    }
}
