namespace zms9110750.WarframeMarketApi.Requests;

/// <summary>
/// 关闭部分或全部订单的请求体
/// </summary>
/// <param name="Quantity">要关闭的单位数量，1 ~ 9999，须能被 order.perTrade 整除</param>
public record CloseOrderRequest(
	int Quantity
);
