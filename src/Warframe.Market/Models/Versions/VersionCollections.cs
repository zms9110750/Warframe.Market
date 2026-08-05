namespace zms9110750.WarframeMarketApi.Models.Versions;

/// <summary>
/// 各数据集合的版本哈希值
/// </summary>
/// <param name="Items">物品集合哈希</param>
/// <param name="Rivens">裂罅集合哈希</param>
/// <param name="Liches">巫妖集合哈希</param>
/// <param name="Sisters">姐妹集合哈希</param>
/// <param name="Missions">任务集合哈希</param>
/// <param name="Npcs">NPC 集合哈希</param>
/// <param name="Locations">位置集合哈希</param>
public record VersionCollections(
    string Items,
    string Rivens,
    string Liches,
    string Sisters,
    string Missions,
    string Npcs,
    string Locations
);
