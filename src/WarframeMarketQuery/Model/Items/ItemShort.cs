namespace WarframeMarketQuery.Model.Items;
/// <summary>
/// 游戏中的物品简要信息
/// </summary>
/// <param name="Id">物品的唯一标识符</param>
/// <param name="Slug">物品的URL友好名称</param>
/// <param name="GameRef">道具在游戏中的路径</param>
/// <param name="Tags">物品的标签列表</param>
/// <param name="MaxRank">物品可达到的最大等级</param>
/// <param name="Vaulted">物品是否已入库</param>
/// <param name="Ducats">物品的杜卡特值</param>
/// <param name="MaxAmberStars">物品的最大琥珀星数量</param>
/// <param name="MaxCyanStars">物品的最大蓝星数量</param>
/// <param name="BaseEndo">物品的基础内融核心值</param>
/// <param name="EndoMultiplier">物品的内融核心值乘数</param>
/// <param name="Subtypes">物品的子类型</param>
/// <param name="I18n">物品的多语言信息，键为语言代码，值为对应的翻译信息</param>
public record ItemShort(
	 string Id,
	 string Slug,
	 string GameRef,
	 HashSet<string> Tags,
	 int? MaxRank,
	 bool? Vaulted,
	 int? Ducats,
	 int? MaxAmberStars,
	 int? MaxCyanStars,
	 int? BaseEndo,
	 float? EndoMultiplier,
	 HashSet<ItemSubtypes>? Subtypes,
	 Dictionary<Language, LanguagePake> I18n
	 )
{
	/// <summary>
	/// 物品类型
	/// </summary>
	public ItemType ItemType
	{
		get
		{
			if (field == default)
			{
				field = Tags is not { Count: > 0 } ? ItemType.Item
					: Tags.Contains("riven") ? ItemType.RivenMOD
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

	public static implicit operator string(ItemShort item)
	{
		return item.Slug;
	}
}