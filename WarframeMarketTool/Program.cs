// 测试客户端过滤盲怒 Top 订单
// API 返回 5买(全rank=10)+5卖(全rank=0)
// 客户端按 rank 过滤会怎样？

using var raw = new HttpClient { BaseAddress = new Uri("https://api.warframe.market") };
raw.DefaultRequestHeaders.Add("Language", "zh-hans");
raw.DefaultRequestHeaders.Add("Platform", "pc");

var resp = await raw.GetStringAsync("/v2/orders/item/blind_rage/top");
var j = System.Text.Json.JsonDocument.Parse(resp);
var data = j.RootElement.GetProperty("data");

var buys = data.GetProperty("buy").EnumerateArray().ToList();
var sells = data.GetProperty("sell").EnumerateArray().ToList();

Console.WriteLine($"全部: 买={buys.Count} 卖={sells.Count}");
Console.WriteLine($"买 rank 分布: {string.Join(", ", buys.Select(b => b.GetProperty("rank").GetRawText()))}");
Console.WriteLine($"卖 rank 分布: {string.Join(", ", sells.Select(b => b.GetProperty("rank").GetRawText()))}");

Console.WriteLine("\n--- 客户端过滤效果 ---");
for (int r = 0; r <= 10; r++)
{
	var fb = buys.Where(b => b.GetProperty("rank").GetInt32() == r).Count();
	var fs = sells.Where(b => b.GetProperty("rank").GetInt32() == r).Count();
	var display = fb > 0 || fs > 0 ? "" : " ← 空";
	Console.WriteLine($"rank={r,2}: 买={fb} 卖={fs}{display}");
}

Console.WriteLine("\n结论: 盲怒的 Top 只覆盖 rank=0(卖) 和 rank=10(买)");
Console.WriteLine("中间等级过滤出来是 0 条，用户看到空表");
