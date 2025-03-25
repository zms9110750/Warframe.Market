namespace Warframe.Market.Model.Versions;
public record Version(
		[property: JsonPropertyName("apiVersion"), JsonProperty("apiVersion")] string ApiVersion,
		[property: JsonPropertyName("data"), JsonProperty("data")] Data Data,
		[property: JsonPropertyName("error"), JsonProperty("error")] string Error);