namespace zms9110750.WarframeMarketApi.Models.Items;

/// <summary>
/// 套装物品信息，包含根物品 ID 和所有部件
/// 如果物品不属于任何套装，则 items 数组只包含该物品自身
/// </summary>
/// <param name="Id">套装根物品 ID（虚拟套装道具）</param>
/// <param name="Items">套装中的物品列表</param>
public record ItemSet(
	string Id,
	Item[] Items
);
