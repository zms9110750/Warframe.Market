using System.Text.Json;
using System.Text.Json.Serialization;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Statistics;

var raw = await new HttpClient { BaseAddress = new Uri("https://api.warframe.market") }
	.GetStringAsync("/v1/items/blind_rage/statistics");

Console.Error.WriteLine("=== 测试 API 库的 Period 反序列化 ===");
var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
var stat = JsonSerializer.Deserialize<Statistic>(raw, opts);
var entries = stat?.Payload?.StatisticsLive?.Day90;
if (entries != null && entries.Length > 0)
{
	Console.Error.WriteLine($"  90days: {entries.Length} 条 ✅");
	var sell = entries.Where(e => e.OrderType == "sell").ToArray();
	Console.Error.WriteLine($"  卖单: {sell.Length} 条");
	Console.Error.WriteLine($"  均价范围: {sell.Min(e => e.AvgPrice):F0} ~ {sell.Max(e => e.AvgPrice):F0}");
	Console.Error.WriteLine($"  均价(简单平均): {sell.Average(e => e.AvgPrice):F0}");
	Console.Error.WriteLine($"  加权均价(按交易量): {sell.Sum(e => e.AvgPrice * e.Volume) / sell.Sum(e => e.Volume):F0}");
	Console.Error.WriteLine($"  中位数范围: {sell.Min(e => e.Median):F0} ~ {sell.Max(e => e.Median):F0}");
}
else
{
	Console.Error.WriteLine($"  ❌ Day90 为 null");
	Console.Error.WriteLine($"  Payload={stat?.Payload != null} StatisticsLive={stat?.Payload?.StatisticsLive != null} Hour48={stat?.Payload?.StatisticsLive?.Hour48?.Length}");
}

Console.Error.WriteLine("\n=== 盲怒参考价格计算 ===");
if (entries != null && entries.Length > 0)
{
	var sells = entries.Where(e => e.OrderType == "sell").ToArray();
	// 参考价 = 加权平均（按交易量）
	var refPrice = sells.Sum(e => e.AvgPrice * e.Volume) / sells.Sum(e => e.Volume);
	Console.Error.WriteLine($"  参考价(加权平均): {refPrice:F0}");

	// 满级价 (ModRank=10)
	var maxRank = sells.Where(e => e.ModRank == 10).ToArray();
	if (maxRank.Any())
	{
		var maxPrice = maxRank.Sum(e => e.AvgPrice * e.Volume) / maxRank.Sum(e => e.Volume);
		Console.Error.WriteLine($"  满级价(R10): {maxPrice:F0}");
	}
}
