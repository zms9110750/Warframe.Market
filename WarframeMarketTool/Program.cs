// 检查 Refit 实际发送了啥
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Orders;
using System.Net.Http;
using System.Text.Json;

// 自己发 HTTP 请求看清 URL
var raw = new HttpClient { BaseAddress = new Uri("https://api.warframe.market") };
raw.DefaultRequestHeaders.Add("Language", "zh-hans");
raw.DefaultRequestHeaders.Add("Platform", "pc");

Console.WriteLine("=== 直接 HTTP 请求 ===");

// 无参数
var url0 = "/v2/orders/item/blind_rage/top";
var resp0 = await raw.GetStringAsync(url0);
var j0 = JsonDocument.Parse(resp0);
var buy0 = j0.RootElement.GetProperty("data").GetProperty("buy").GetArrayLength();
var sell0 = j0.RootElement.GetProperty("data").GetProperty("sell").GetArrayLength();
Console.WriteLine($"无参数: Buy={buy0}, Sell={sell0}");

// 带 Rank=0
var url1 = "/v2/orders/item/blind_rage/top?Rank=0";
var resp1 = await raw.GetStringAsync(url1);
var j1 = JsonDocument.Parse(resp1);
var buy1 = j1.RootElement.GetProperty("data").GetProperty("buy").GetArrayLength();
var sell1 = j1.RootElement.GetProperty("data").GetProperty("sell").GetArrayLength();
Console.WriteLine($"Rank=0:  Buy={buy1}, Sell={sell1}");

// 带 RankLt=0
var url2 = "/v2/orders/item/blind_rage/top?RankLt=0";
var resp2 = await raw.GetStringAsync(url2);
var j2 = JsonDocument.Parse(resp2);
var buy2 = j2.RootElement.GetProperty("data").GetProperty("buy").GetArrayLength();
var sell2 = j2.RootElement.GetProperty("data").GetProperty("sell").GetArrayLength();
Console.WriteLine($"RankLt=0: Buy={buy2}, Sell={sell2}");

// 验证返回的数据中的 rank 分布
Console.WriteLine("\n=== RankLt=0 的数据 ===");
var entries = j2.RootElement.GetProperty("data");
foreach (var b in entries.GetProperty("buy").EnumerateArray().Take(5))
{
	var r = b.GetProperty("rank").GetRawText();
	var p = b.GetProperty("platinum").GetInt32();
	Console.WriteLine($"  买 rank={r} price={p}");
}
foreach (var s in entries.GetProperty("sell").EnumerateArray().Take(5))
{
	var r = s.GetProperty("rank").GetRawText();
	var p = s.GetProperty("platinum").GetInt32();
	Console.WriteLine($"  卖 rank={r} price={p}");
}

Console.WriteLine("\n=== 完成 ===");
