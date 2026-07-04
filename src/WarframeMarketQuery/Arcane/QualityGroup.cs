using System.Collections;
using WarframeMarketQuery.Model.Items;

namespace WarframeMarketQuery.Arcane;
public record QualityGroup(ItemSubtypes Subtypes, double Quality, string[] Items) : IGrouping<ItemSubtypes, string>
{
	public double QualityEach => Quality / Items.Length;

	ItemSubtypes IGrouping<ItemSubtypes, string>.Key => Subtypes;

	IEnumerator<string> IEnumerable<string>.GetEnumerator() => (Items as IEnumerable<string>).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => (Items as IEnumerable<string>).GetEnumerator();
}
