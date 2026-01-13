namespace WarframeMarketQuery.Model.Versions;
/// <summary>
/// 版本数据
/// </summary>
/// <param name="Id">Id</param>
/// <param name="UpdatedAt">更新时间</param>
public record Version(
	 string Id,
	 DateTime UpdatedAt);