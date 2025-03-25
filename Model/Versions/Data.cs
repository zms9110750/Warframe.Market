namespace Warframe.Market.Model.Versions;

public record Data(
	[property: JsonPropertyName("id"), JsonProperty("id")] string Id,
	[property: JsonPropertyName("updatedAt"), JsonProperty("updatedAt")] DateTime UpdatedAt);