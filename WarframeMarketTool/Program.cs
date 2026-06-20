using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Orders;

var wfm = new WarframeMarketClient { Crossplay = true, Language = zms9110750.WarframeMarketApi.Models.Items.Language.ZhHans, Platform = zms9110750.WarframeMarketApi.Models.Users.Platform.PC };

Console.WriteLine("=== 对比 Rank 精确 vs RankLt (上限) ===");

async Task T(string label, int? rank, int? rankLt)
{
    var r = await wfm.GetOrdersItemTopAsync("high_voltage",
        new(Rank: rank, RankLt: rankLt, Charges: null, ChargesLt: null,
            AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null, Subtype: null));
    var d = r?.Content?.Data;
    if (d == null) { Console.WriteLine($"{label}: null"); return; }
    Console.WriteLine($"{label}: 买={string.Join(",", (d.Buy??[]).Select(b=>$"R{b.Rank}${b.Platinum}"))}");
}

Console.WriteLine("--- 基准 ---");
await T("无参数", null, null);

Console.WriteLine("\n--- Rank 精确 ---");
await T("rank=0", 0, null);
await T("rank=1", 1, null);
await T("rank=2", 2, null);
await T("rank=3", 3, null);

Console.WriteLine("\n--- RankLt (上限 ≤ ) ---");
await T("rankLt=0", null, 0);
await T("rankLt=1", null, 1);
await T("rankLt=2", null, 2);
await T("rankLt=3", null, 3);
