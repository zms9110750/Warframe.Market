using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Warframe.Market.Helper;
using Warframe.Market.Model.Items;
using Warframe.Market.Model.Statistics;

namespace Warframe.Market.Model.LocalItems;
/// <summary>
/// 游戏中的物品简要信息
/// </summary>
/// <param name="Id">物品的唯一标识符</param>
/// <param name="Slug">物品的URL友好名称</param>
/// <param name="GameRef">道具在游戏中的路径</param>
/// <param name="Tags">物品的标签列表</param>
/// <param name="I18n">物品的多语言信息，键为语言代码，值为对应的翻译信息</param>
/// <param name="MaxRank">物品可达到的最大等级</param>
/// <param name="Vaulted">物品是否已入库</param>
/// <param name="Ducats">物品的杜卡特值</param>
/// <param name="MaxAmberStars">物品的最大琥珀星数量</param>
/// <param name="MaxCyanStars">物品的最大蓝星数量</param>
/// <param name="BaseEndo">物品的基础内融核心值</param>
/// <param name="EndoMultiplier">物品的内融核心值乘数</param>
/// <param name="Subtypes">物品的子类型</param>
public record ItemShort(
	[property: JsonPropertyName("id"), JsonProperty("id")] string Id,
	[property: JsonPropertyName("slug"), JsonProperty("slug")] string Slug,
	[property: JsonPropertyName("gameRef"), JsonProperty("gameRef")] string GameRef,
	[property: JsonPropertyName("tags"), JsonProperty("tags")] HashSet<string> Tags,
	[property: JsonPropertyName("maxRank"), JsonProperty("maxRank")] int? MaxRank,
	[property: JsonPropertyName("vaulted"), JsonProperty("vaulted")] bool? Vaulted,
	[property: JsonPropertyName("ducats"), JsonProperty("ducats")] int? Ducats,
	[property: JsonPropertyName("maxAmberStars"), JsonProperty("maxAmberStars")] int? MaxAmberStars,
	[property: JsonPropertyName("maxCyanStars"), JsonProperty("maxCyanStars")] int? MaxCyanStars,
	[property: JsonPropertyName("baseEndo"), JsonProperty("baseEndo")] int? BaseEndo,
	[property: JsonPropertyName("endoMultiplier"), JsonProperty("endoMultiplier")] float? EndoMultiplier,
	[property: JsonPropertyName("subtypes"), JsonProperty("subtypes")] HashSet<Subtypes>? Subtypes)
{
	/// <summary>
	/// 物品的多语言信息，键为语言代码，值为对应的翻译信息
	/// </summary>
	[property: JsonPropertyName("i18n"), JsonProperty("i18n")]
	public ItemI18n I18n { get; set; } = null!;
	public ItemType ItemType
	{
		get
		{
			if (field == default)
			{
				field = Tags is not { Count: > 0 } ? ItemType.Item
					: Tags.Contains("riven_mod") ? ItemType.RivenMOD
					: Tags.Contains("mod") ? ItemType.MOD
					: Tags.Contains("fish") ? ItemType.Fish
					: Tags.Contains("relic") ? ItemType.Relic
					: Tags.Contains("prime") ? ItemType.PrimeComponent
					: Tags.Contains("arcane_enhancement") ? ItemType.ArcaneEnhancement
					: Tags.Contains("ayatan_sculpture") ? ItemType.AyatanSculpture
					: Tags.Contains("component") || Tags.Contains("set") || Tags.Contains("modular") || this is Item { SetParts: { } } ? ItemType.Component
					: Tags.Contains("weapon") ? ItemType.Equipment
					: ItemType.Item;
			}
			return field;
		}
	}
} 