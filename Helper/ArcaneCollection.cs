using Markdig.Extensions.Tables;
using Markdig;
using System.Collections;
using System.Reflection;
using System.Text;
using Warframe.Market.Model.Subtype;
using Markdig.Syntax;
using Warframe.Market.Helper;
using Warframe.Market.Model.ItemType;

namespace Warframe.Market.Model;

public sealed class ArcaneCollection : ILookup<Quality, string>
{
	public string PackageName { get; init; }
	public const double 荧尘可买赋能包数量 = 420.0 * 6 / 200 * 3;
	private Dictionary<Quality, (double quality, HashSet<string> items)> Lookup { get; } = [];
	public int Count => Lookup.Count;
	public double ReferencePrice { get; private set; }
	public IEnumerable<string> this[Quality key] => Lookup.TryGetValue(key, out var result) ? result.items : [];

	private ArcaneCollection(string packageName)
	{
		PackageName = packageName;
	}
	public static IEnumerable<ArcaneCollection> Created()
	{
		using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Warframe.Market.Configuration.赋能包.md");
		using StreamReader reader = new StreamReader(stream!, Encoding.UTF8);
		var pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
		var tables = Markdown.Parse(reader.ReadToEnd(), pipeline).Descendants<Table>().ToArray();
		var chest = tables[0].Descendants<TableRow>()
			.Select(s => s.Descendants<TableCell>().ToArray()).Skip(1).ToDictionary(s => s[0].ExtractText());
		var probability = tables[1].Descendants<TableRow>()
			.Select(s => s.Descendants<TableCell>().ToArray()).Skip(1).ToDictionary(s => s[0].ExtractText());

		foreach (var item in chest)
		{
			var arcane = new ArcaneCollection(item.Key);
			for (int j = 1; j < 5; j++)
			{
				var set = item.Value[j].ExtractTexts().ToHashSet();
				if (set.Count == 0)
				{
					continue;
				}
				arcane.Lookup.Add((Quality)j, (double.Parse(probability[item.Key][j].ExtractText().Trim('%')) / 100, set));
			}
			yield return arcane;
		}
	}

	public async ValueTask<double> GetReferencePrice(ItemCache itemCache, WMClient client)
	{
		if (ReferencePrice != 0)
		{
			return ReferencePrice;
		}
		Dictionary<Quality, Task<double[]>> dic = [];
		foreach (var item in this)
		{
			var t = Task.WhenAll(item.Select(name => itemCache[name])
				.OfType<ArcaneEnhancement>()
				.Select(s => s.GetReferencePriceFromDisassemble(client)
				.AsTask()));
			dic.Add(item.Key, t);
		}
		double price = 0;
		foreach (var item in dic)
		{
			price += GetProbability(item.Key).eachItem * (await item.Value).Sum();
		}
		ReferencePrice = price;
		return ReferencePrice;
	}
	public (double quality, double eachItem) GetProbability(Quality quality)
	{
		return Lookup.TryGetValue(quality, out var result)
			? ((double quality, double eachItem))(result.Item1, result.Item1 / result.Item2.Count)
			: default;
	}

	public static IAsyncEnumerable<(string, double)> ShowReferencePrice(ItemCache itemCache, WMClient client)
	{
		return Created().ToAsyncEnumerable()
				.SelectAwait(async s => (s.PackageName, price: 荧尘可买赋能包数量 * await s.GetReferencePrice(itemCache, client)))
				.OrderByDescending(s => s.Item2);
	}

	public IEnumerator<IGrouping<Quality, string>> GetEnumerator()
	{
		return Lookup
			.SelectMany(s => s.Value.items
			, (a, b) => (a.Key, b))
			.GroupBy(b => b.Key, b => b.b).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool Contains(Quality key)
	{
		return Lookup.ContainsKey(key);
	}
}
