using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Orders;

var wfm = new WarframeMarketClient { Crossplay = true, Language = zms9110750.WarframeMarketApi.Models.Items.Language.ZhHans, Platform = zms9110750.WarframeMarketApi.Models.Users.Platform.PC };

Console.WriteLine("=== 验证 ===");
async Task T(string label, int? rank, int? rankLt)
{
    var r = await wfm.GetOrdersItemTopAsync("high_voltage",
        new(Rank: rank, RankLt: rankLt, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null, Subtype: null));
    var b = r?.Content?.Data?.Buy?.Select(x => $"R{x.Rank}${x.Platinum}");
    Console.WriteLine($"{label,-15}: 买=[{string.Join(",", b ?? [])}]");
}
await T("无参数", null, null);
await T("rank=0", 0, null);
await T("rank=3", 3, null);
