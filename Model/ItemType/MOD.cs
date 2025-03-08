using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Warframe.Market.Helper;
namespace Warframe.Market.Model.ItemType;

/// <summary>
/// MOD基础类型（包含等级和稀有度）
/// </summary>
/// <param name="MaxRank">物品的最大等级</param>
/// <param name="Rarity">物品的稀有度</param>
public record MOD(
	[property: JsonPropertyName("maxRank"), JsonProperty("maxRank")] sbyte MaxRank,
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
	public ValueTask<double> GetMaxRankReferencePrice(WMClient client, IEnumerable<double>? weight = null)
	{
		return GetReferencePrice(client, s => s.ModRank == MaxRank, weight);
	}
	public ValueTask<double> GetReferencePrice(WMClient client, IEnumerable<double>? weight = null)
	{
		return GetReferencePrice(client, m => m.ModRank == 0, weight);
	}
}
