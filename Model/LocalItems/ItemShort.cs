using Newtonsoft.Json.Linq;
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
	[property: JsonPropertyName("i18n"), JsonProperty("i18n")] Dictionary<string, ItemI18n> I18n,
	[property: JsonPropertyName("maxRank"), JsonProperty("maxRank")] int? MaxRank,
	[property: JsonPropertyName("vaulted"), JsonProperty("vaulted")] bool? Vaulted,
	[property: JsonPropertyName("ducats"), JsonProperty("ducats")] int? Ducats,
	[property: JsonPropertyName("maxAmberStars"), JsonProperty("maxAmberStars")] int? MaxAmberStars,
	[property: JsonPropertyName("maxCyanStars"), JsonProperty("maxCyanStars")] int? MaxCyanStars,
	[property: JsonPropertyName("baseEndo"), JsonProperty("baseEndo")] int? BaseEndo,
	[property: JsonPropertyName("endoMultiplier"), JsonProperty("endoMultiplier")] float? EndoMultiplier,
	[property: JsonPropertyName("subtypes"), JsonProperty("subtypes")] HashSet<Subtypes>? Subtypes)
{
	public static IReadOnlyList<int> SyntheticConsumption { get; } = [1, 3, 6, 10, 15, 21];
	public WMClient? WMClient { get; set; }
	public string? PriceCachePath { get; set => field = value == null ? null : Path.Combine(value, Slug+ ".json"); }
	private Statistic Price { get; set; } = default!;

	public async Task<Statistic> GetPriceAsync()
	{
		if (Price != null)
		{
			return Price;
		}
		if (File.Exists(PriceCachePath) && File.GetLastWriteTimeUtc(PriceCachePath).Date == DateTime.UtcNow.Date)
		{
			using var read = File.OpenText(PriceCachePath);
			await using var jsonRead = new JsonTextReader(read);
			try
			{
				var json = await JObject.LoadAsync(jsonRead);
				return Price = json.ToObject<Statistic>()!;
			}
			catch (JsonReaderException)
			{
			}
		}
		ArgumentNullException.ThrowIfNull(WMClient);
		Price = await WMClient.GetStatisticsAsync(this);
		if (PriceCachePath != null)
		{
			await using var write = File.CreateText(PriceCachePath);
			await using var jsonWriter = new JsonTextWriter(write);
			await JObject.FromObject(Price).WriteToAsync(jsonWriter);
		}
		return Price;
	}
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
	public ItemI18n I18nZH => I18n["zh-hans"];
	public ItemI18n I18nEN => I18n["en"];
	public Func<Entry, bool>? PriceFilterDefault()
	{
		return ItemType switch
		{
			ItemType.ArcaneEnhancement => s => s.ModRank == 0,
			ItemType.AyatanSculpture => s => s.AmberStars != 0,
			ItemType.CraftedComponent => s => s.Subtype == Items.Subtypes.Blueprint,
			ItemType.MOD => s => s.ModRank == 0,
			ItemType.Relic => s => s.Subtype == Items.Subtypes.Intact,
			ItemType.RivenMOD => s => s.Subtype == Items.Subtypes.Unrevealed,
			_ => null,
		};
	}
	public Func<Entry, bool>? PriceFilterMaxRank()
	{
		return ItemType switch
		{
			ItemType.ArcaneEnhancement => s => s.ModRank != 0,
			ItemType.MOD => s => s.ModRank != 0,
			_ => PriceFilterDefault(),
		};
	}
	public Func<Entry, bool>? PriceFilterSubtype(Subtypes subtypes)
	{
		return Subtypes is { Count: > 0 } ? (s => s.Subtype == subtypes) : PriceFilterDefault();
	}
}
