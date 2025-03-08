
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Warframe.Market.Helper;
namespace Warframe.Market.Model.ItemType;

/// <summary>
/// 虚空遗物（包含入库状态）
/// </summary>
/// <param name="Subtypes">物品的子类型列表</param>
/// <param name="Vaulted">指示物品是否已入库</param>
public record Relic(
	[property: JsonPropertyName("subtypes"), JsonProperty("subtypes")] HashSet<Subtype.Relic> Subtypes,
	[property: JsonPropertyName("vaulted"), JsonProperty("vaulted")] bool Vaulted,
	string Id,
	string UrlName,
	string Slug,
	string GameRef,
	HashSet<string> Tags,
	int TradingTax,
	Dictionary<string, ItemI18n> I18n)
	: Item(Id, UrlName, Slug, GameRef, Tags, TradingTax, I18n)
{
	public ValueTask<double> GetReferencePrice(WMClient client, Subtype.Relic subtypes, IEnumerable<double>? weight = null)
	{
		return GetReferencePrice<Subtype.Relic>(client, subtypes, weight);
	}
}

