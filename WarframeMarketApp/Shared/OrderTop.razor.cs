using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Masa.Blazor;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Orders;

namespace WarframeMarketApp.Shared;

public partial class OrderTop : ComponentBase, IDisposable
{
	[Inject] private WarframeMarketClient Wfm { get; set; } = null!;
	[Inject] private IJSRuntime Js { get; set; } = null!;

	[CascadingParameter(Name = "ClickLink")] public bool ClickLink { get; set; }
	[Parameter] public ItemShort? Item { get; set; }

	protected bool loading = true;
	protected Order[] TopOrder = Array.Empty<Order>();
	protected List<DataTableHeader<Order>> _headers = new()
	{
		new("买/卖", nameof(Order.Type)),
		new("价格", nameof(Order.Platinum)),
		new("数量", nameof(Order.Quantity)),
		new("等级", nameof(Order.Rank)),
		new("卖家", nameof(Order.User)) { ValueExpression = (Func<Order, object?>)(r => r.User?.IngameName) },
		new("联系", "msg") { Sortable = false },
	};

	protected override async Task OnInitializedAsync()
	{
		if (Item == null) return;
		loading = true;
		try
		{
			var top = await Wfm.GetOrdersItemTopAsync(Item.Slug, null);
			if (top?.Content?.Data != null)
				TopOrder = top.Content.Data.Sell.Concat(top.Content.Data.Buy).ToArray();
		}
		finally { loading = false; }
	}

	protected async Task CopyAsync(string text)
	{
		try { await Js.InvokeVoidAsync("navigator.clipboard.writeText", text); }
		catch { }
	}

	public void Dispose() { }
}
