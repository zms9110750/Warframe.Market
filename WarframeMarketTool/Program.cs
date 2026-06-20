// 测 API 到底认什么格式的 rank 参数
using System.Net.Http;
using System.Text.Json;

var raw = new HttpClient { BaseAddress = new Uri("https://api.warframe.market") };
raw.DefaultRequestHeaders.Add("Language", "zh-hans");
raw.DefaultRequestHeaders.Add("Platform", "pc");

async Task Test(string label, string url)
{
    try
    {
        var resp = await raw.GetStringAsync(url);
        var j = JsonDocument.Parse(resp);
        var buys = j.RootElement.GetProperty("data").GetProperty("buy").EnumerateArray().ToList();
        var sells = j.RootElement.GetProperty("data").GetProperty("sell").EnumerateArray().ToList();
        var buyRanks = string.Join(",", buys.Select(b => $"R{b.GetProperty("rank")}${b.GetProperty("platinum")}"));
        var sellRanks = string.Join(",", sells.Select(s => $"R{s.GetProperty("rank")}${s.GetProperty("platinum")}"));
        Console.WriteLine($"{label,-25}: 买=[{buyRanks}] 卖=[{sellRanks}]");
    }
    catch (Exception ex) { Console.WriteLine($"{label,-25}: {ex.Message}"); }
}

var slug = "high_voltage";
Console.WriteLine("=== high_voltage Rank/RankLt 参数格式测试 ===\n");

Console.WriteLine("--- 基准 ---");
await Test("无参数", $"/v2/orders/item/{slug}/top");

Console.WriteLine("\n--- Rank 精确 ---");
await Test("rank=0", $"/v2/orders/item/{slug}/top?rank=0");
await Test("rank=3", $"/v2/orders/item/{slug}/top?rank=3");

Console.WriteLine("\n--- snake_case: rank_lt ---");
await Test("rank_lt=0", $"/v2/orders/item/{slug}/top?rank_lt=0");
await Test("rank_lt=1", $"/v2/orders/item/{slug}/top?rank_lt=1");
await Test("rank_lt=2", $"/v2/orders/item/{slug}/top?rank_lt=2");
await Test("rank_lt=3", $"/v2/orders/item/{slug}/top?rank_lt=3");

Console.WriteLine("\n--- kebab-case: rank-lt ---");
await Test("rank-lt=0", $"/v2/orders/item/{slug}/top?rank-lt=0");
await Test("rank-lt=1", $"/v2/orders/item/{slug}/top?rank-lt=1");
await Test("rank-lt=2", $"/v2/orders/item/{slug}/top?rank-lt=2");
await Test("rank-lt=3", $"/v2/orders/item/{slug}/top?rank-lt=3");

Console.WriteLine("\n--- PascalCase: RankLt ---");
await Test("RankLt=0", $"/v2/orders/item/{slug}/top?RankLt=0");
await Test("RankLt=3", $"/v2/orders/item/{slug}/top?RankLt=3");

Console.WriteLine("\n--- camelCase: rankLt ---");
await Test("rankLt=0", $"/v2/orders/item/{slug}/top?rankLt=0");
await Test("rankLt=3", $"/v2/orders/item/{slug}/top?rankLt=3");
