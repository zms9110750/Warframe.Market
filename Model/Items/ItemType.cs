namespace Warframe.Market.Model.Items;

public enum ItemType
{
	None = 0,
	///<summary> 物品</summary>
	Item = 1 << 0,
	///<summary> 赋能</summary>
	ArcaneEnhancement = 1 << 1 | Item,
	///<summary> 内融核心塑像</summary>
	AyatanSculpture = 1 << 2 | Item,
	///<summary> 装备</summary>
	Equipment = 1 << 5 | Item,
	///<summary> 组件</summary>
	Component = 1 << 3 | Equipment,
	///<summary> 可制作组件</summary>
	CraftedComponent = 1 << 4 | Component,
	///<summary> 鱼</summary>
	Fish = 1 << 6 | Item,
	///<summary> MOD</summary>
	MOD = 1 << 7 | Item,
	///<summary> Prime部件</summary>
	PrimeComponent = 1 << 8 | Component,
	///<summary> 虚空遗物</summary>
	Relic = 1 << 9 | Item,
	///<summary> 裂罅MOD</summary>
	RivenMOD = 1 << 10 | Item
}
