// ═══════════════════════════════════════════
// WarframeMarketTool — 测试沙盒
// 先验证算法再搬进 App，最终不留。
// ═══════════════════════════════════════════

using System.Text.Json;
using zms9110750.WarframeMarketApi.Models.Statistics;

var raw = await new HttpClient { BaseAddress = new Uri("https://api.warframe.market") }
	.GetStringAsync("/v1/items/blind_rage/statistics");

var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
var stat = JsonSerializer.Deserialize<Statistic>(raw, opts);

if (stat?.Payload?.StatisticsClosed?.Day90 == null) { Console.Error.WriteLine("无数据"); return; }

var day90 = stat.Payload.StatisticsClosed.Day90;
Console.Error.WriteLine($"Day90 共 {day90.Length} 条, 日期: {day90.Min(e => e.Datetime):yyyy-MM-dd} ~ {day90.Max(e => e.Datetime):yyyy-MM-dd}");

// 0 级价
static double? Calc(double[] ws, Entry[] entries) {
	var tw = 0.0; var ws_ = 0.0;
	for (int i = 0; i < entries.Length; i++) {
		var w = ws[i] * entries[i].Volume;
		tw += w; ws_ += w * entries[i].Median;
	}
	return tw > 0 ? ws_ / tw : null;
}
double[] ws = [40, 25, 15, 5, 5, 5, 5];

var r0 = day90.Where(e => e.ModRank is null or 0).OrderByDescending(e => e.Datetime).Take(7).ToArray();
Console.Error.WriteLine($"\n0 级价 ({r0.Length} 天): {Calc(ws, r0)?.ToString("F1")}");

var maxRanked = day90.Where(e => e.ModRank > 0).OrderByDescending(e => e.ModRank).FirstOrDefault();
if (maxRanked != null) Console.Error.WriteLine($"\n最高 ModRank={maxRanked.ModRank}");

// 满级价：ModRank > 0, subtype in (null, crafted, radiant, magnificent, large)
var max = day90.Where(e => e.ModRank > 0 && (e.Subtype is null or "crafted" or "radiant" or "magnificent" or "large"))
	.OrderByDescending(e => e.Datetime).Take(7).ToArray();
if (max.Length > 0) {
	Console.Error.WriteLine($"满级价 ({max.Length} 天, subtype 分布: {string.Join(", ", max.Select(m => m.Subtype ?? "null").Distinct())}): {Calc(ws, max)?.ToString("F1")}");
} else {
	// fallback: 直接按最高等级
	var fallback = day90.Where(e => e.ModRank > 0).OrderByDescending(e => e.Datetime).Take(7).ToArray();
	if (fallback.Length > 0) Console.Error.WriteLine($"满级价(fallback) ({fallback.Length} 天): {Calc(ws, fallback)?.ToString("F1")}");
}

// 混合价
static int SynCons(int? rank) => rank switch { 1 => 3, 2 => 6, 3 => 10, 4 => 15, 5 => 21, _ => 1 };
var firstRanked = day90.FirstOrDefault(e => e.ModRank > 0);
if (firstRanked != null)
{
	var syn = SynCons(firstRanked.ModRank);
	var maxP = Calc(ws, max.Length > 0 ? max : day90.Where(e => e.ModRank > 0).OrderByDescending(e => e.Datetime).Take(7).ToArray());
	Console.Error.WriteLine($"\n混合价 (ModRank={firstRanked.ModRank}, 消耗={syn}): 满级价/{syn} = {maxP / syn:F1}");
}

Console.Error.WriteLine("\n=== 完成 ===");
