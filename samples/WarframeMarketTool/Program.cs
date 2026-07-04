// 全面测试 high_voltage (满级3级) 的各种参数组合
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Orders;

var wfm = new WarframeMarketClient
{
    Crossplay = true,
    Language = zms9110750.WarframeMarketApi.Models.Items.Language.ZhHans,
    Platform = zms9110750.WarframeMarketApi.Models.Users.Platform.PC
};

var slug = "high_voltage";
Console.WriteLine($"=== {slug} ===\n");

async Task Test(string label, int? rank, int? rankLt)
{
    var r = await wfm.GetOrdersItemTopAsync(slug,
        new(Rank: rank, RankLt: rankLt, Charges: null, ChargesLt: null,
            AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null, Subtype: null));
    var d = r?.Content?.Data;
    if (d == null) { Console.WriteLine($"{label}: null"); return; }

    var buys = string.Join(" | ", (d.Buy ?? []).Select(b => $"R{b.Rank}$P{b.Platinum}"));
    var sells = string.Join(" | ", (d.Sell ?? []).Select(s => $"R{s.Rank}$P{s.Platinum}"));
    Console.WriteLine($"{label,-14} 买=[{buys}]");
    Console.WriteLine($"{"",-14} 卖=[{sells}]");
}

Console.WriteLine("--- 基准（无参数）---");
await Test("基准", null, null);

Console.WriteLine("\n--- 精确 Rank ---");
await Test("Rank=0", 0, null);
await Test("Rank=1", 1, null);
await Test("Rank=2", 2, null);
await Test("Rank=3", 3, null);

Console.WriteLine("\n--- RankLt（≤N）---");
await Test("Lt=0", null, 0);
await Test("Lt=1", null, 1);
await Test("Lt=2", null, 2);
await Test("Lt=3", null, 3);

Console.WriteLine("\n--- 分析 ---");
Console.WriteLine("购(看买单): 我要买的订单，显示的是别人想卖给我的价格(卖单)");
Console.WriteLine("  基准卖单分布：看卖单数据");
Console.WriteLine("  滑块=0：显示全部（0到max）→ 无过滤");
Console.WriteLine("  滑块=1：显示1到max → rank≥1 → 客户端过滤 RankLt=max 再取 ≥1");
Console.WriteLine("  滑块=3：显示3到max → rank≥3 → 客户端过滤 RankLt=max 再取 ≥3");
Console.WriteLine("售(看卖单): 我看别人在卖什么价");
Console.WriteLine("  基准买单分布：看买单数据");
Console.WriteLine("  滑块=0：显示全部（0到max）→ 无过滤");
Console.WriteLine("  滑块=1：显示0到1 → rank≤1 → RankLt=1");
Console.WriteLine("  滑块=3：显示0到3 → rank≤3 → RankLt=3");

Console.WriteLine("\n=== 满级5级卡: split_flights ===");
slug = "split_flights";
Console.WriteLine($"--- 基准 ---");
await Test("基准", null, null);
Console.WriteLine("--- RankLt ---");
await Test("Lt=1", null, 1);
await Test("Lt=2", null, 2);
await Test("Lt=3", null, 3);
await Test("Lt=4", null, 4);
await Test("Lt=5", null, 5);

Console.WriteLine("\n=== 结论 ===");
Console.WriteLine("购(买单): 滑块=N → 本地过滤 Rank ≥ N（API 没有 ≥ 的参数）");
Console.WriteLine("售(卖单): 滑块=N → RankLt=N（API 支持 ≤ 的参数）");
