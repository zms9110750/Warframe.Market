using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Orders;

var wfm = new WarframeMarketClient { Crossplay = true, Language = zms9110750.WarframeMarketApi.Models.Items.Language.ZhHans, Platform = zms9110750.WarframeMarketApi.Models.Users.Platform.PC };

var mods = new[] { "high_voltage", "split_flights", "catalyzing_shields", "streamline", "fleeting_expertise" };

foreach (var mod in mods)
{
    Console.WriteLine($"\n=== {mod} ===");
    var def = await wfm.GetOrdersItemTopAsync(mod, null);
    var db = def?.Content?.Data?.Buy ?? [];
    var ds = def?.Content?.Data?.Sell ?? [];
    Console.WriteLine($"  默认: 买={db.Length}({string.Join(",",db.Select(b=>$"R{b.Rank}${b.Platinum}"))}) 卖={ds.Length}({string.Join(",",ds.Select(s=>$"R{s.Rank}${s.Platinum}"))})");

    // Rank=0
    var r0 = await wfm.GetOrdersItemTopAsync(mod, new(Rank: 0, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null, Subtype: null));
    var r0b = r0?.Content?.Data?.Buy ?? [];
    Console.WriteLine($"  Rank=0: 买={r0b.Length}({string.Join(",", r0b.Select(b=>$"R{b.Rank}${b.Platinum}"))})");

    // Max rank (取默认数据中最高 rank)
    var maxRank = db.Concat(ds).Max(o => o.Rank ?? 0);
    Console.WriteLine($"  最高等级: {maxRank}");

    // RankLt=全部
    var rLt = await wfm.GetOrdersItemTopAsync(mod, new(Rank: null, RankLt: maxRank, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null, Subtype: null));
    var rLtB = rLt?.Content?.Data?.Buy ?? [];
    Console.WriteLine($"  RankLt={maxRank}: 买={rLtB.Length}({string.Join(",", rLtB.Select(b=>$"R{b.Rank}${b.Platinum}"))})");

    // 对比：RankLt 结果是否等于 默认
    var same = rLtB.Length == db.Length && rLtB.Zip(db).All(p => p.First.Platinum == p.Second.Platinum);
    Console.WriteLine($"  RankLt==默认: {same}");
}
