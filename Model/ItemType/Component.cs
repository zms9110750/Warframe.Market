using System.Text.Json.Serialization;
using Newtonsoft.Json;
namespace Warframe.Market.Model.ItemType;

/// <summary>
/// 套装组件类型
/// </summary>
/// <param name="SetRoot">指示物品是否为套装根物品</param>
/// <param name="SetParts">物品所属套装的部件列表</param>
/// <param name="QuantityInSet">物品在套装中的数量</param>
public record Component(
	[property: JsonPropertyName("setRoot"), JsonProperty("setRoot")] bool SetRoot,
	[property: JsonPropertyName("setParts"), JsonProperty("setParts")] HashSet<string> SetParts,
	[property: JsonPropertyName("quantityInSet"), JsonProperty("quantityInSet")] sbyte QuantityInSet,
	sbyte ReqMasteryRank,
	string Id,
	string UrlName,
	string Slug,
	string GameRef,
	HashSet<string> Tags,
	int TradingTax,
	Dictionary<string, ItemI18n> I18n)
	: Equipment(ReqMasteryRank, Id, UrlName, Slug, GameRef, Tags, TradingTax, I18n);
