using Warframe.Market.Model.Items;

namespace Warframe.Market.Model.ItemsSet;

public record Data(
	[property: JsonPropertyName("id"), JsonProperty("id")] string Id,
	[property: JsonPropertyName("items"), JsonProperty("items")] Item[] Items);
