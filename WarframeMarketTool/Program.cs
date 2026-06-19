using System.Text.Json;
using System.Text.Json.Serialization;
using zms9110750.WarframeMarketApi.Models.Statistics;

var raw = await new HttpClient { BaseAddress = new Uri("https://api.warframe.market") }
	.GetStringAsync("/v1/items/blind_rage/statistics");

var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
var stat = JsonSerializer.Deserialize<Statistic>(raw, opts);

Console.Error.WriteLine("=== StatisticsClosed (已结算) ===");
var closed = stat?.Payload?.StatisticsClosed?.Day90;
if (closed != null)
{
	Console.Error.WriteLine($"Day90 共 {closed.Length} 条");
	Console.Error.WriteLine($"  ModRank 分布: {string.Join(", ", closed.GroupBy(e => e.ModRank ?? 0).OrderBy(g => g.Key).Select(g => $"R{g.Key}={g.Count()}"))}");
	Console.Error.WriteLine($"  第一/最后一条: {closed.Min(e => e.Datetime):yyyy-MM-dd} ~ {closed.Max(e => e.Datetime):yyyy-MM-dd}");

	// R0 的卖单
	var r0 = closed.Where(e => e.ModRank is null or 0).ToArray();
	Console.Error.WriteLine($"\n  R0 共 {r0.Length} 条");
	if (r0.Any())
	{
		Console.Error.WriteLine($"  均价范围: {r0.Min(e => e.AvgPrice):F1} ~ {r0.Max(e => e.AvgPrice):F1}");
		Console.Error.WriteLine($"  中位数范围: {r0.Min(e => e.Median):F1} ~ {r0.Max(e => e.Median):F1}");
		Console.Error.WriteLine($"  最近7天均价: {r0.Where(e => e.Datetime > DateTime.UtcNow.AddDays(-7)).Select(e => e.AvgPrice).DefaultIfEmpty(0).Average():F1}");
	}

	// R10 的卖单
	var r10 = closed.Where(e => e.ModRank == 10).ToArray();
	Console.Error.WriteLine($"\n  R10 共 {r10.Length} 条");
	if (r10.Any())
	{
		Console.Error.WriteLine($"  均价范围: {r10.Min(e => e.AvgPrice):F1} ~ {r10.Max(e => e.AvgPrice):F1}");
		Console.Error.WriteLine($"  中位数范围: {r10.Min(e => e.Median):F1} ~ {r10.Max(e => e.Median):F1}");
	}
}

Console.Error.WriteLine("\n\n=== StatisticsLive (实时) ===");
var live = stat?.Payload?.StatisticsLive?.Day90;
if (live != null)
{
	Console.Error.WriteLine($"Day90 共 {live.Length} 条");
	var sells = live.Where(e => e.OrderType == "sell").ToArray();
	var buys = live.Where(e => e.OrderType == "buy").ToArray();
	Console.Error.WriteLine($"  卖单: {sells.Length} 条  买单: {buys.Length} 条");

	Console.Error.WriteLine($"\n  卖单 R0 均价: {sells.Where(e => e.ModRank is null or 0).Select(e => e.AvgPrice).DefaultIfEmpty(0).Average():F1}");
	Console.Error.WriteLine($"  卖单 R10 均价: {sells.Where(e => e.ModRank == 10).Select(e => e.AvgPrice).DefaultIfEmpty(0).Average():F1}");
	Console.Error.WriteLine($"  买单 R10 均价: {buys.Where(e => e.ModRank == 10).Select(e => e.AvgPrice).DefaultIfEmpty(0).Average():F1}");
}
