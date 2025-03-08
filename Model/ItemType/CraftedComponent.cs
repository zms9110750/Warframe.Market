using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Warframe.Market.Helper;
namespace Warframe.Market.Model.ItemType;

/// <summary>
/// 可制作组件（包含子类型）
/// </summary>
/// <param name="Subtypes">物品的子类型列表</param>
public record CraftedComponent(
	[property: JsonPropertyName("subtypes"), JsonProperty("subtypes")] HashSet<Subtype.Component> Subtypes,
	bool SetRoot,
	HashSet<string> SetParts,
	sbyte QuantityInSet,
	sbyte ReqMasteryRank,
	string Id,
	string UrlName,
	string Slug,
	string GameRef,
	HashSet<string> Tags,
	int TradingTax,
	Dictionary<string, ItemI18n> I18n)
	: Component(SetRoot, SetParts, QuantityInSet, ReqMasteryRank, Id, UrlName, Slug, GameRef, Tags, TradingTax, I18n)
{
	public   ValueTask<double> GetReferencePrice(WMClient client, Subtype.Component subtypes, IEnumerable<double>? weight = null)
	{
	  return GetReferencePrice<Subtype.Component>(client, subtypes, weight);
	}
}
