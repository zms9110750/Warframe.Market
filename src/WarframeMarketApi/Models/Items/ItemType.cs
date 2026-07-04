namespace zms9110750.WarframeMarketApi.Models.Items;

/// <summary>
/// 物品在游戏中的类型分类，通过 Tags 推断
/// </summary>
public enum ItemType
{
	/// <summary>占位符</summary>
	None = 0,
	/// <summary>普通物品</summary>
	Item = 1,
	/// <summary>赋能</summary>
	ArcaneEnhancement = 2,
	/// <summary>安魂雕塑</summary>
	AyatanSculpture = 3,
	/// <summary>装备/武器</summary>
	Equipment = 4,
	/// <summary>组件</summary>
	Component = 5,
	/// <summary>可制作组件</summary>
	CraftedComponent = 6,
	/// <summary>鱼</summary>
	Fish = 7,
	/// <summary>MOD</summary>
	MOD = 8,
	/// <summary>Prime 部件</summary>
	PrimeComponent = 9,
	/// <summary>虚空遗物</summary>
	Relic = 10,
	/// <summary>裂罅 MOD</summary>
	RivenMOD = 11,
}
