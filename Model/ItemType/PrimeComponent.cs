using System.Text.Json.Serialization;
using Newtonsoft.Json;
namespace Warframe.Market.Model.ItemType;

/// <summary>
/// Prime部件（包含杜卡德价值）
/// </summary>
/// <param name="Ducats">物品的杜卡德金币数量</param>
public record PrimeComponent(
	[property: JsonPropertyName("ducats"), JsonProperty("ducats")] short Ducats,
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
	: Component(SetRoot, SetParts, QuantityInSet, ReqMasteryRank, Id, UrlName, Slug, GameRef, Tags, TradingTax, I18n);
