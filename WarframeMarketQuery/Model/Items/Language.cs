namespace WarframeMarketQuery.Model.Items;

/// <summary>
/// 物品的语言符号
/// </summary> 
public enum Language
{
	/// <summary>
	/// 占位符
	/// </summary>
	Node,
	/// <summary>
	/// 英语
	/// </summary>
	En = 1 << 1,
	/// <summary>
	/// 韩语
	/// </summary>
	Ko = 1 << 2,
	/// <summary>
	/// 俄语
	/// </summary>
	Ru = 1 << 3,
	/// <summary>
	/// 德语
	/// </summary>
	De = 1 << 4,
	/// <summary>
	/// 法语
	/// </summary>
	Fr = 1 << 5,
	/// <summary>
	/// 葡萄牙语
	/// </summary>
	Pt = 1 << 6,
	/// <summary>
	/// 西班牙语
	/// </summary>
	Es = 1 << 7,
	/// <summary>
	/// 意大利语
	/// </summary>
	It = 1 << 8,
	/// <summary>
	/// 波兰语
	/// </summary>
	Pl = 1 << 9,
	/// <summary>
	/// 乌克兰语
	/// </summary>
	Uk = 1 << 10,
	/// <summary>
	/// 简体中文
	/// </summary>
	ZhHans = 1 << 11,
	/// <summary>
	/// 繁体中文
	/// </summary>
	ZhHant = 1 << 12,
}