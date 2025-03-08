using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Warframe.Market.Helper;
namespace Warframe.Market.Model.ItemType;

/// <summary>
/// 内融核心塑像
/// </summary>
/// <param name="MaxAmberStars">物品的最大琥珀星数量</param>
/// <param name="MaxCyanStars">物品的最大青星数量</param>
/// <param name="BaseEndo">物品的基础内融核心数量</param>
/// <param name="EndoMultiplier">物品的内融核心倍率</param>
public record AyatanSculpture(
	[property: JsonPropertyName("maxAmberStars"), JsonProperty("maxAmberStars")] sbyte MaxAmberStars,
	[property: JsonPropertyName("maxCyanStars"), JsonProperty("maxCyanStars")] sbyte MaxCyanStars,
	[property: JsonPropertyName("baseEndo"), JsonProperty("baseEndo")] short BaseEndo,
	[property: JsonPropertyName("endoMultiplier"), JsonProperty("endoMultiplier")] float EndoMultiplier,
	string Id,
	string UrlName,
	string Slug,
	string GameRef,
	HashSet<string> Tags,
	int TradingTax,
	Dictionary<string, ItemI18n> I18n)
	: Item(Id, UrlName, Slug, GameRef, Tags, TradingTax, I18n)
{
	public   ValueTask<double> GetMaxStarsReferencePrice(WMClient client, IEnumerable<double>? weight = null)
	{
		return GetReferencePrice(client, s => s.CyanStars == MaxCyanStars && s.AmberStars == MaxAmberStars, weight); 
	}
}