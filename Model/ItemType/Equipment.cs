using System.Text.Json.Serialization;
using Newtonsoft.Json;
namespace Warframe.Market.Model.ItemType;

/// <summary>
/// 装备类型（需要精通等级）
/// </summary>
/// <param name="ReqMasteryRank">物品所需的最低精通段位</param>
public record Equipment(
	[property: JsonPropertyName("reqMasteryRank"), JsonProperty("reqMasteryRank")] sbyte ReqMasteryRank,
	string Id,
	string UrlName,
	string Slug,
	string GameRef,
	HashSet<string> Tags,
	int TradingTax,
	Dictionary<string, ItemI18n> I18n)
	: Item(Id, UrlName, Slug, GameRef, Tags, TradingTax, I18n);
