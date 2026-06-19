using System.Text.Json;
using System.Text.Json.Serialization;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Statistics;

var raw = await new HttpClient { BaseAddress = new Uri("https://api.warframe.market") }
	.GetStringAsync("/v1/items/blind_rage/statistics");

var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
var stat = JsonSerializer.Deserialize<Statistic>(raw, opts);

Console.Error.WriteLine("=== 原版参考价算法复现 ===");

var closed = stat?.Payload?.StatisticsClosed?.Day90;
if (closed == null || closed.Length == 0) { Console.Error.WriteLine("无数据"); return; }

Console.Error.WriteLine($"Day90 共 {closed.Length} 条");

// 过滤器（原版）
var filtered = closed
	.Where(s => s.ModRank is not > 0 && s.AmberStars is not > 0 &&
				(s.Subtype == null || (s.Subtype != "crafted" && s.Subtype != "radiant")))
	.OrderByDescending(x => x.Datetime)
	.ToArray();

Console.Error.WriteLine($"过滤后: {filtered.Length} 条");

// 原版 DefaultWeight = [40, 25, 15, 5, 5, 5, 5]
double[] weights = [40, 25, 15, 5, 5, 5, 5];

// 原版算法
var totalWeight = 0.0;
var weightedSum = 0.0;
int count = 0;
foreach (var entry in filtered.Take(7))
{
	var w = weights[count];
	var volWeight = entry.Volume * w;
	totalWeight += volWeight;
	weightedSum += volWeight * entry.Median;
	Console.Error.WriteLine($"  [{count}] 时间={entry.Datetime:MM-dd HH:mm} 均价={entry.AvgPrice,6:F0} 中位数={entry.Median,5:F0} 交易量={entry.Volume,3} 权{w,2} 加权中位={entry.Median * w,6:F0}");
	count++;
}

var refPrice = weightedSum / totalWeight;
Console.Error.WriteLine($"\n参考价(原版算法): {refPrice:F0}");

// 对比简单平均
Console.Error.WriteLine($"简单均价(卖单): {filtered.Where(e => e.OrderType == "sell").Select(e => e.AvgPrice).DefaultIfEmpty(0).Average():F0}");
Console.Error.WriteLine($"加权均价(按量): {filtered.Sum(e => e.AvgPrice * e.Volume) / filtered.Sum(e => e.Volume):F0}");
