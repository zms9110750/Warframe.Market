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

    private static Statistic MakeRelicStat(params (string subtype, double median, int volume, string dt)[] entries)
    {
        var arr = entries.Select(e => new Entry(
            DateTime.Parse(e.dt), e.volume, 0, 0, 0, 0, (float)e.median, null,
            e.dt, 0, e.subtype, null, null, null, null, null, null)).ToArray();
        return new Statistic(new Payload(
            new Period(Array.Empty<Entry>(), arr),   // Day90 = arr
            new Period(Array.Empty<Entry>(), Array.Empty<Entry>())));
    }

    [Fact]
    public void Relic_prices_map_radiant_to_max_and_intact_to_reference()
    {
        // 遗物统计：90days 按 subtype 区分精炼度（intact/exceptional/flawless/radiant）
        var stat = MakeRelicStat(
            ("intact", 5, 100, "2026-08-01T00:00:00Z"),
            ("intact", 6, 80, "2026-08-02T00:00:00Z"),
            ("radiant", 20, 40, "2026-08-01T00:00:00Z"),
            ("radiant", 22, 30, "2026-08-02T00:00:00Z"),
            ("exceptional", 9, 50, "2026-08-01T00:00:00Z"));

        // 满级价=光辉（radiant）、参考价=完整（intact）——互不混入
        Assert.NotNull(stat.GetRelicRadiantPrice());
        Assert.True(stat.GetRelicRadiantPrice() > 15);   // radiant 档（20/22 加权）
        Assert.NotNull(stat.GetRelicIntactPrice());
        Assert.True(stat.GetRelicIntactPrice() < 10);    // intact 档（5/6 加权）
        // 默认（不限 subtype）方法会混入全部档位——遗物专用方法与默认不一致
        Assert.NotEqual(stat.GetRelicIntactPrice(), stat.GetReferencePrice());
    }
}
