#define 赋能详情
#define 赋能包价格
using Autofac;
using System.Xml.Linq;
using Warframe.Market.Helper;
using Warframe.Market.Extend;
using Warframe.Market.Helper.Abstract;
using Warframe.Market.Helper.AutofacModule;


var builder = new ContainerBuilder();
builder.RegisterModule<ArcanePackageModule>();
builder.RegisterModule<ResilientHttpModule>();
builder.RegisterModule<WClientModule>();
await using var con = builder.Build();
var wmclient = con.Resolve<IWMClient>();
var cache = await wmclient.GetItemsCacheAsync();
var pack = con.Resolve<ArcanePackage[]>();

#if 赋能包价格
await foreach (var item in Task.WhenEach(pack.Select(p => SelectReferencePrice(p, 0))))
{
	var a = await item;
	Console.WriteLine(a.Item1.Name + " " + a.Item2);
}
Console.Clear();
Console.Write("组/每日收购");
int[] nums = [0, 2, 6, 15];
foreach (var num in nums)
{
	Console.Write($"{num,6},");
}
Console.WriteLine();
var a2 = await Task.WhenAll(pack.Select(p => SelectReferencePrice(p, 15)));

foreach (var item in a2.OrderByDescending(s => s.Item2))
{
	string paddedName = item.Item1.Name;
	var b = char.IsAscii(paddedName[0]);
	paddedName += new string(' ', 9 - paddedName.Length * (b ? 1 : 2));

	Console.Write($"{paddedName}：");
	for (int i = 0; i < nums.Length; i++)
	{
		Console.Write($"{SelectReferencePrice(item.Item1, nums[i]).Result.Item2,6:f1},");
	}
	Console.WriteLine();
}
Console.WriteLine();
Console.WriteLine();
Console.WriteLine();
async Task<(ArcanePackage, double)> SelectReferencePrice(ArcanePackage p, int n)
{
	return (p, await p.GetReferencePrice(cache, wmclient, n));
}
#endif
#if 赋能详情
foreach (var item in pack.OrderBy(s => s.SelectMany(d => d).Count()))
{
	List<XElement> list = [];
	foreach (var itemshort in item.SelectMany(s => s).Select(s => cache[s]))
	{
		var st = await wmclient.GetStatisticsAsync(itemshort);
		XElement e = new XElement(item.Name + "·" + itemshort.I18n.ZhHans.Name[3..5]);
		e.SetAttributeValue("概率", (100 * item.GetProbability(itemshort.I18n.ZhHans.Name)).ToString("f1"));
		var volume = st.Payload.StatisticsClosed.Day90.Sum(s => s.Volume * StatisticExtend.SyntheticConsumption[s.ModRank ?? 0]) / 90.0;
		e.SetAttributeValue("交易量", volume.ToString(volume > 100 ? "f0" : "f1"));
		e.SetAttributeValue("满级", st.GetMaxRankReferencePrice(itemshort).ToString("f1"));
		e.SetAttributeValue("概率x零级", (item.GetProbability(itemshort.I18n.ZhHans.Name) * st.GetMaterialBasedReferencePrice(itemshort)).ToString("f2"));
		list.Add(e);
	}
	list.Sort((a, b) => ((double)b.Attribute("交易量")).CompareTo((double)a.Attribute("交易量")));
	foreach (var e in list)
	{
		Console.WriteLine(e);
	}
	Console.WriteLine();
}
#endif
