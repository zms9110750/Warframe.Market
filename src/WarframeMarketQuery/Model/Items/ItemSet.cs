namespace WarframeMarketQuery.Model.Items;

/// <summary>
/// 套装物品信息
/// </summary>
/// <param name="Id">根物品的id（虚拟的套装道具）</param>
/// <param name="Items">部件的信息（包括虚拟的套装道具）</param>
/// <remarks>如果一个物品不属于套装，那么他本身会作为根物品</remarks>
public record ItemSet(
	 string Id,
	 Item[] Items);
