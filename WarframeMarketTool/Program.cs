// 测试 AliasAs 修复后的 Refit 请求
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Orders;

var wfm = new WarframeMarketClient
{
	Crossplay = true,
	Language = zms9110750.WarframeMarketApi.Models.Items.Language.ZhHans,
	Platform = zms9110750.WarframeMarketApi.Models.Users.Platform.PC
};

Console.WriteLine("=== 修复后验证 ===");

// 无参数
var r0 = await wfm.GetOrdersItemTopAsync("blind_rage", null);
Console.WriteLine($"无参数: Buy={r0?.Content?.Data?.Buy?.Length} Sell={r0?.Content?.Data?.Sell?.Length}");

// Rank=0
var r1 = await wfm.GetOrdersItemTopAsync("blind_rage",
	new(Rank: 0, RankLt: null, Charges: null, ChargesLt: null,
		AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null, Subtype: null));
var b1 = r1?.Content?.Data?.Buy;
Console.WriteLine($"Rank=0:  Buy={b1?.Length} 首买rank={b1?.FirstOrDefault()?.Rank}");

// Rank=10
var r2 = await wfm.GetOrdersItemTopAsync("blind_rage",
	new(Rank: 10, RankLt: null, Charges: null, ChargesLt: null,
		AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null, Subtype: null));
var b2 = r2?.Content?.Data?.Buy;
Console.WriteLine($"Rank=10: Buy={b2?.Length} 首买rank={b2?.FirstOrDefault()?.Rank}");

// RankLt=1
var r3 = await wfm.GetOrdersItemTopAsync("blind_rage",
	new(Rank: null, RankLt: 1, Charges: null, ChargesLt: null,
		AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null, Subtype: null));
var b3 = r3?.Content?.Data?.Buy;
Console.WriteLine($"RankLt=1: Buy={b3?.Length} 首买rank={b3?.FirstOrDefault()?.Rank}");

Console.WriteLine("\n=== 完成 ===");
