namespace zms9110750.WarframeMarketApi.Models.Items;

/// <summary>
/// 物品子类型集合。继承 <see cref="HashSet{T}"/> 可直接 JSON 序列化。
/// 提供布尔属性快速判断物品类别。
/// </summary>
public class ItemSubtypeSet : HashSet<string>
{
    public ItemSubtypeSet() : base(StringComparer.OrdinalIgnoreCase) { }

    // ===== 快捷属性 =====

    /// <summary>是否为裂罅 MOD</summary>
    public bool IsRiven => Overlaps(RivenKeywords);
    private static HashSet<string> RivenKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "rivenmod", "riven_mod", "riven" };

    /// <summary>是否为已揭示裂罅</summary>
    public bool IsRevealed => Contains("revealed");

    /// <summary>是否为未揭示裂罅</summary>
    public bool IsUnrevealed => Contains("unrevealed");

    /// <summary>是否为 MOD</summary>
    public bool IsMod => Overlaps(ModKeywords);
    private static HashSet<string> ModKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "mod", "rivenmod", "riven_mod" };

    /// <summary>是否为虚空遗物</summary>
    public bool IsRelic => Overlaps(RelicKeywords);
    private static HashSet<string> RelicKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "relic", "intact", "exceptional", "flawless", "radiant" };

    /// <summary>是否为鱼类</summary>
    public bool IsFish => Overlaps(FishKeywords);
    private static HashSet<string> FishKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "fish", "small", "medium", "large" };

    /// <summary>是否为宝石</summary>
    public bool IsGem => Contains("gem");

    /// <summary>是否为安魂雕塑</summary>
    public bool IsAyatan => Contains("ayatan_sculpture");

    /// <summary>是否为赋能</summary>
    public bool IsArcane => Overlaps(ArcaneKeywords);
    private static HashSet<string> ArcaneKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "arcane_enhancement", "arcane" };

    /// <summary>是否为 Prime 部件</summary>
    public bool IsPrimeComponent => Overlaps(PrimeKeywords);
    private static HashSet<string> PrimeKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "prime_component", "prime" };

    /// <summary>是否为蓝图</summary>
    public bool IsBlueprint => Contains("blueprint");

    /// <summary>是否为成品/制造品</summary>
    public bool IsCrafted => Contains("crafted");

    /// <summary>是否为组件</summary>
    public bool IsComponent => Overlaps(ComponentKeywords);
    private static HashSet<string> ComponentKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "component", "blueprint", "crafted" };

    /// <summary>是否为装备/武器</summary>
    public bool IsWeapon => Overlaps(WeaponKeywords);
    private static HashSet<string> WeaponKeywords { get; } = new(StringComparer.OrdinalIgnoreCase) { "weapon", "primary", "secondary", "melee", "archwing", "arch-gun", "arch-melee" };
}
