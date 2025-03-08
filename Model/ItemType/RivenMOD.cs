using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Warframe.Market.Helper;
namespace Warframe.Market.Model.ItemType;

/// <summary>
/// 裂罅MOD（包含子类型）
/// </summary>
/// <param name="Subtypes">物品的子类型列表</param> 
/// <param name="Rarity">物品的稀有度</param>
public record RivenMOD(
	[property: JsonPropertyName("subtypes"), JsonProperty("subtypes")] HashSet<Subtype.RivenMod> Subtypes,
	[property: JsonPropertyName("rarity"), JsonProperty("rarity")] string Rarity,
	string Id,
	string UrlName,
	string Slug,
	string GameRef,
	HashSet<string> Tags,
	int TradingTax,
	Dictionary<string, ItemI18n> I18n)
	: Item(Id, UrlName, Slug, GameRef, Tags, TradingTax, I18n)
{
	public ValueTask<double> GetReferencePrice(WMClient client, Subtype.RivenMod subtypes, IEnumerable<double>? weight = null)
	{
		return GetReferencePrice<Subtype.RivenMod>(client, subtypes, weight);
	}
}


