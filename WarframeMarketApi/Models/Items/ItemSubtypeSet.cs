namespace zms9110750.WarframeMarketApi.Models.Items;

/// <summary>
/// 物品子类型集合。继承 <see cref="HashSet{T}"/> 可直接 JSON 序列化。
/// 提供布尔属性快速判断物品类别。
/// </summary>
public class ItemSubtypeSet : HashSet<string>
{
	public ItemSubtypeSet() : base(StringComparer.OrdinalIgnoreCase) { }

	// ===== 静态查找集 =====

	public static HashSet<string> RivenKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "rivenmod", "riven_mod", "riven" };
	public static HashSet<string> ModKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "mod", "rivenmod", "riven_mod" };
	public static HashSet<string> RelicKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "relic", "intact", "exceptional", "flawless", "radiant" };
	public static HashSet<string> FishKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "fish", "small", "medium", "large" };
	public static HashSet<string> WeaponKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "weapon", "primary", "secondary", "melee", "archwing", "arch-gun", "arch-melee" };
	public static HashSet<string> ComponentKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "component", "blueprint", "crafted" };
	public static HashSet<string> ArcaneKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "arcane_enhancement", "arcane" };
	public static HashSet<string> PrimeKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "prime_component", "prime" };
	public static HashSet<string> RevealedKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "revealed" };
	public static HashSet<string> UnrevealedKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "unrevealed" };
	public static HashSet<string> GemKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "gem" };
	public static HashSet<string> AyatanKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "ayatan_sculpture" };
	public static HashSet<string> BlueprintKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "blueprint" };
	public static HashSet<string> CraftedKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "crafted" };

	// ===== 快捷属性 =====

	public bool IsRiven => Overlaps(RivenKeywords);
	public bool IsRevealed => Overlaps(RevealedKeywords);
	public bool IsUnrevealed => Overlaps(UnrevealedKeywords);
	public bool IsMod => Overlaps(ModKeywords);
	public bool IsRelic => Overlaps(RelicKeywords);
	public bool IsFish => Overlaps(FishKeywords);
	public bool IsGem => Overlaps(GemKeywords);
	public bool IsAyatan => Overlaps(AyatanKeywords);
	public bool IsArcane => Overlaps(ArcaneKeywords);
	public bool IsPrimeComponent => Overlaps(PrimeKeywords);
	public bool IsBlueprint => Overlaps(BlueprintKeywords);
	public bool IsCrafted => Overlaps(CraftedKeywords);
	public bool IsComponent => Overlaps(ComponentKeywords);
	public bool IsWeapon => Overlaps(WeaponKeywords);
}
