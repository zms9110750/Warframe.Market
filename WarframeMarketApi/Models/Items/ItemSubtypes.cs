namespace zms9110750.WarframeMarketApi.Models.Items;

/// <summary>
/// 物品子类型（裂罅类型、遗物品质、鱼大小、组件状态等）
/// </summary>
public enum ItemSubtypes
{
	/// <summary>占位符 / 未知类型</summary>
	Node = 0,
	/// <summary>裂罅 MOD</summary>
	RivenMod = 0x10,
	/// <summary>已揭示裂罅</summary>
	Revealed,
	/// <summary>未揭示裂罅</summary>
	Unrevealed,
	/// <summary>虚空遗物</summary>
	Relic = 0x20,
	/// <summary>完整遗物</summary>
	Intact,
	/// <summary>优良遗物</summary>
	Exceptional,
	/// <summary>无暇遗物</summary>
	Flawless,
	/// <summary>光辉遗物</summary>
	Radiant,
	/// <summary>品质</summary>
	Quality = 0x30,
	/// <summary>常见</summary>
	Common,
	/// <summary>罕见</summary>
	Uncommon,
	/// <summary>稀有</summary>
	Rare,
	/// <summary>传说</summary>
	Legendary,
	/// <summary>鱼</summary>
	Fish = 0x40,
	/// <summary>小鱼</summary>
	Small,
	/// <summary>中鱼</summary>
	Medium,
	/// <summary>大鱼</summary>
	Large,
	/// <summary>基础级</summary>
	Basic,
	/// <summary>装饰级</summary>
	Adorned,
	/// <summary>华丽级</summary>
	Magnificent,
	/// <summary>可制作组件</summary>
	Component = 0x50,
	/// <summary>蓝图</summary>
	Blueprint,
	/// <summary>成品</summary>
	Crafted,
}
