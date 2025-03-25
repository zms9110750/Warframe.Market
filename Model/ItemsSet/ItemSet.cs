namespace Warframe.Market.Model.ItemsSet;
public record ItemSet(
	[property: JsonPropertyName("apiVersion"), JsonProperty("apiVersion")] string ApiVersion,
	[property: JsonPropertyName("data"), JsonProperty("data")] Data Data,
	[property: JsonPropertyName("error"), JsonProperty("error")] string Error);
