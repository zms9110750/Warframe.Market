using Microsoft.AspNetCore.Components;
using Masa.Blazor;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Orders;
using zms9110750.WarframeMarketApi.Models.Users;
using zms9110750.WarframeMarketApi.Models.Statistics;
using WarframeMarketApp.Services;

namespace WarframeMarketApp.Pages;

public partial class UserSearch : ComponentBase, IDisposable
{
	[Inject] private ItemsService ItemSvc { get; set; } = null!;
	[Inject] private WarframeMarketClient Wfm { get; set; } = null!;

	protected string slug = "";
	protected User? user;
	protected List<Order>? orders;
	protected bool loading;
	protected bool searched;
	protected bool loadingPrices;
	protected string? error;
	private CancellationTokenSource _cts = new();
	private Dictionary<string, Statistic?> _prices = new();
	protected List<DataTableHeader<Order>> _headers = new()
	{
		new("物品", "item"),
		new("类型", "Type"),
		new("铂金", nameof(Order.Platinum)),
		new("数量", nameof(Order.Quantity)),
		new("等级", nameof(Order.Rank)),
		new("参考价", "ref"),
		new("差价", "diff"),
	};

	protected async Task SearchAsync()
	{
		if (string.IsNullOrWhiteSpace(slug)) return;
		loading = true; searched = true; error = null;
		user = null; orders = null; _prices.Clear();
		_cts?.Cancel(); _cts = new();

		try
		{
			var userResp = await Wfm.GetUserAsync(slug);
			user = userResp?.Content?.Data;

			var orderResp = await Wfm.GetOrdersFromUserAsync(slug);
			if (orderResp?.Content?.Data == null || orderResp.Content.Data.Length == 0) { searched = true; return; }

			orders = orderResp.Content.Data.ToList();
			loading = false;
			loadingPrices = true;
			_ = LoadPricesAsync();
		}
		catch (Exception ex) { error = ex.Message; loading = false; }
	}

	private async Task LoadPricesAsync()
	{
		foreach (var o in orders!)
		{
			if (_cts.IsCancellationRequested) break;
			var itemSlug = o.ItemId ?? "";
			var stat = await ItemSvc.GetStatisticAsync(itemSlug);
			_prices[itemSlug] = stat;
			StateHasChanged();
			await Task.Delay(200);
		}
		loadingPrices = false; StateHasChanged();
	}

	protected string GetItemName(Order o) => o.ItemId?.Length > 16 ? o.ItemId[..16] + "..." : o.ItemId ?? "-";
	protected string GetRef(Order o) => GetPriceStr(o.ItemId);
	protected string GetDiff(Order o) => GetDiffStr(o.ItemId, o.Platinum);

	private string GetPriceStr(string? slug)
	{
		if (slug == null || !_prices.TryGetValue(slug, out var stat) || stat == null) return loadingPrices ? "" : "-";
		var p = ItemSvc.GetReferencePrice(stat);
		return p?.ToString("F0") ?? "-";
	}

	private string GetDiffStr(string? slug, int orderPrice)
	{
		if (slug == null || !_prices.TryGetValue(slug, out var stat) || stat == null) return "";
		var refP = ItemSvc.GetReferencePrice(stat);
		if (refP == null || refP <= 0) return "";
		var diff = orderPrice - refP.Value;
		return diff >= 0 ? $"+{diff:F0}" : $"{diff:F0}";
	}

	public void Dispose() { _cts?.Cancel(); _cts?.Dispose(); }
}
