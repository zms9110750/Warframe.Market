using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Newtonsoft.Json.Linq;
namespace Warframe.Market;

public static class TableExtensions
{
	static TableExtensions()
	{ 
		var dic = JObject.Parse(File.ReadAllText("X:\\lang.json")).ToObject<Dictionary<string, string>>(); 
		foreach (var item in dic)
		{
			try
			{
				Translate.Add(item.Key, item.Value);
			}
			catch
			{
			}
		} 
	}

	public const string PackageContent = """
| 赋能组合包 | 常见 | 罕见 | 稀有 | 传说 |
| --- | --- | --- | --- | --- |
| 科维兽 |  |   Melee Fortification <br/> Melee Retaliation | Melee Animosity <br/> Melee Exposure <br/> Melee Influence <br/> Melee Vortex | Melee Crescendo <br/> Melee Duplicate |
| 双衍王境 |  | Arcane Intention <br/> Magus Aggress | Arcane Power Ramp <br/> Primary Blight <br/> Primary Exhilarate <br/> Primary Obstruct <br/> Shotgun Vendetta <br/> Akimbo Slip Shot <br/> Secondary Outburst | Arcane Reaper <br/> Longbow Sharpshot <br/> Secondary Shiver |
| 夜灵 | Arcane Consequence <br/> Arcane Ice <br/> Arcane Momentum <br/> Arcane Nullifier <br/> Arcane Tempo <br/> Arcane Warmth | Arcane Acceleration <br/> Arcane Agility <br/> Arcane Awakening <br/> Arcane Deflection <br/> Arcane Eruption <br/> Arcane Guardian <br/> Arcane Healing <br/> Arcane Phantasm <br/> Arcane Resistance <br/> Arcane Strike <br/> Arcane Trickery <br/> Arcane Velocity <br/> Arcane Victory |   Arcane Aegis <br/> Arcane Arachne <br/> Arcane Avenger <br/> Arcane Fury <br/> Arcane Precision <br/> Arcane Pulse <br/> Arcane Rage <br/> Arcane Ultimatum <br/>| Arcane Barrier <br/> Arcane Energize <br/> Arcane Grace |
| 坚守者 |  |  | Arcane Blessing <br/> Arcane Rise <br/> Molt Augmented <br/> Molt Efficiency <br/> Molt Reconstruct <br/> Molt Vigor <br/> Fractalized Reset <br/> Primary Frostbite <br/> Cascadia Accuracy <br/> Cascadia Empowered <br/> Cascadia Flare <br/> Cascadia Overcharge <br/> Conjunction Voltage <br/> Emergence Dissipate <br/> Emergence Renewed <br/> Emergence Savior <br/> Eternal Eradicate <br/> Eternal Logistics <br/> Eternal Onslaught |  |
| 殁世幽都 |  |  |   Arcane Double Back <br/> Arcane Steadfast <br/> Theorem Contagion <br/> Theorem Demulcent <br/> Theorem Infection <br/> Primary Plated Round <br/> Secondary Encumber <br/> Secondary Kinship <br/> Residual Boils <br/> Residual Malodor <br/> Residual Shock <br/> Residual Viremia |
| Ostron | Magus Husk <br/> Magus Vigor <br/> Virtuos Null <br/> Virtuos Tempo | Exodia Triumph <br/> Exodia Valor <br/> Magus Cadence <br/> Magus Cloud <br/> Magus Replenish <br/> Virtuos Fury <br/> Virtuos Strike | Exodia Brave <br/> Exodia Force <br/> Exodia Hunt <br/> Exodia Might <br/> Magus Elevate <br/> Magus Nourish <br/> Virtuos Ghost <br/> Virtuos Shadow |  |
| 索拉里斯 | Magus Accelerant <br/> Magus Anomaly <br/> Magus Drive <br/> Magus Firewall <br/> Magus Overload <br/> Virtuos Spike <br/> Virtuos Surge | Magus Glitch <br/> Magus Repair <br/> Virtuos Forge <br/> Virtuos Trojan |   Pax Bolt <br/> Pax Charge <br/> Pax Seeker <br/> Pax Soar <br/> Magus Destruct <br/> Magus Lockdown <br/> Magus Melt <br/> Magus Revert |
| 钢铁 |  |  | Arcane Blade Charger <br/> Arcane Bodyguard <br/> Arcane Pistoleer <br/> Arcane Primary Charger <br/> Arcane Tanker <br/> Primary Deadhead <br/> Primary Dexterity <br/> Primary Merciless <br/> Secondary Deadhead <br/> Secondary Dexterity <br/> Secondary Merciless | |
""";
	public const string PackageProbability = """
| 赋能组合包 | 常见 | 罕见 | 稀有 | 传说 |
| --- | --- | --- | --- | --- |
| 双衍王境	|		|45% 	|50%	|5%
| 科维兽	|		|45%	|50%	|5%
| 夜灵		|	40%	|35%	|20%	|5%
| 坚守者	|		|		|100%	|
| 殁世幽都	|		|		|100%	|
| 钢铁		|		|		|100%	|
| Ostron	|	10%	|30%	|60%	|
| 索拉里斯	|	15%	|15%	|70%	|
""";
	public const double 荧尘可买赋能包数量 = 420.0 * 6 / 200 * 3;
	/// <summary>
	/// 将 Markdig 的 Table 对象转换为 ILookup<(列头, 行头), 单元格内容>
	/// </summary>
	public static ILookup<(string RowHeader, string ColumnHeader), string> ToLookup(this Table table)
	{
		var entries = new List<(string RowHeader, string ColumnHeader, string cellContent)>();

		var headers = table.Descendants<TableRow>().First().Descendants<TableCell>()
			.Select(c => string.Concat(c.ExtractText()))
			.ToList();

		foreach (var row in table.Descendants<TableRow>().Skip(1))
		{
			string rowHeader = string.Concat(row.Descendants<TableCell>().First().ExtractText());
			foreach (var item in row.Descendants<TableCell>().Index().Skip(1))
			{
				entries.AddRange(item.Item.ExtractText().Select(c => (rowHeader, headers[item.Index], c)));
			}
		}
		return entries.ToLookup(x => (x.RowHeader, x.ColumnHeader), x => x.Item3);
	}
	public static IEnumerable<string> ExtractText(this TableCell cell)
	{
		return cell.Descendants<LiteralInline>()
				.Select(l => l.Content.ToString().Trim())
				.Where(t => !string.IsNullOrEmpty(t));
	}
	static IReadOnlyList<int> 合成消耗 { get; } = [1, 3, 6, 10, 15, 21];
	public static IAsyncEnumerable<(string header, double price)> ArcanePriceAsync()
	{
		var pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
		var table1 = Markdown.Parse(PackageContent, pipeline).Descendants<Table>().First().ToLookup();
		var table2 = Markdown.Parse(PackageProbability, pipeline).Descendants<Table>().First().ToLookup();
		var result = MarketData.GetAveragePrice(table1.SelectMany(x => x))
			.Where(s => s.rank > 0)
			.Select(s => (s.itemName, price: s.price / 合成消耗[s.rank ?? 0]));

		return table1.Join(table2, x => x.Key,
					x => x.Key,
					(a, b) =>
					 a.Select(x => (header: a.Key.RowHeader, probability: double.Parse(b.First().Trim('%')) / 100 / a.Count(), itemNames: x)))
					.SelectMany(x => x)
					.ToAsyncEnumerable()
					.Join(result,
					x => x.itemNames,
					x => x.itemName,
					(a, b) =>
					(a.header, price: b.price * a.probability))
					.GroupBy(s => s.header, s => s.price)
					.SelectAwait(async s => (s.Key, await s.SumAsync() * 荧尘可买赋能包数量))
					.OrderByDescending(s => s.Item2);
	}
	public static BidirectionalDictionary<string, string> Translate { get; } = new BidirectionalDictionary<string, string>();
}