using System.Collections;
using Warframe.Market.Model.ItemType;

namespace Warframe.Market.Model;
public class ItemSet(IEnumerable<Component> components) : IReadOnlyCollection<Component>
{
	public Component[] Components { get; init; } = components.ToArray();

	public int Count => ((IReadOnlyCollection<Component>)Components).Count;

	public IEnumerator<Component> GetEnumerator()
	{
		return ((IEnumerable<Component>)Components).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Components.GetEnumerator();
	}
}
