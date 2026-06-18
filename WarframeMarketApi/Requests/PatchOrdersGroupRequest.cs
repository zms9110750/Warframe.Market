namespace zms9110750.WarframeMarketApi.Requests;

/// <summary>
/// 更新虚拟订单分组可见性的请求体
/// </summary>
/// <param name="Visible">要设置的可见性状态，默认 false</param>
/// <param name="Type">限制更新的订单类型（sell / buy），不传则更新两种</param>
public record PatchOrdersGroupRequest(
	bool Visible = false,
	string? Type = null
);
