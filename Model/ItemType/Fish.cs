using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Warframe.Market.Helper;
namespace Warframe.Market.Model.ItemType;

/// <summary>
/// 鱼类物品（包含子类型）
/// </summary>
/// <param name="Subtypes">物品的子类型列表</param>
public record Fish(
	[property: JsonPropertyName("subtypes"), JsonProperty("subtypes")] HashSet<Subtype.Fish> Subtypes,
	string Id,
	string UrlName,
	string Slug,
	string GameRef,
	HashSet<string> Tags,
	int TradingTax,
	Dictionary<string, ItemI18n> I18n)
	: Item(Id, UrlName, Slug, GameRef, Tags, TradingTax, I18n)
{
	public ValueTask<double> GetReferencePrice(WMClient client, Subtype.Fish subtypes, IEnumerable<double>? weight = null)
	{
		return GetReferencePrice<Subtype.Fish>(client, subtypes, weight);
	}
}


