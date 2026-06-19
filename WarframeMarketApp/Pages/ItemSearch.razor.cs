using Microsoft.AspNetCore.Components;
using Masa.Blazor;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Statistics;
using WarframeMarketApp.Services;

namespace WarframeMarketApp.Pages;

public partial class ItemSearch : ComponentBase, IDisposable
{
	[Inject] private ItemsService ItemSvc { get; set; } = null!;

	protected string query = "";
	protected bool searched;
	protected List<ItemShort> ItemsList = new();
	protected List<DataTableHeader<ItemShort>> _headers = new()
	{
		new("中文名称", "zh"),
		new("英文名称", "en"),
		new("价格", "price") { Align = DataTableHeaderAlign.End },
		new("满级价格", "maxprice") { Align = DataTableHeaderAlign.End },
	};
	private CancellationTokenSource _cts = new();
	private Dictionary<string, Statistic?> _stats = new();
	private Dictionary<string, double?> _prices = new();
	private Dictionary<string, double?> _maxPrices = new();

	protected async Task DoSearch()
	{
		if (string.IsNullOrWhiteSpace(query)) return;
		searched = true;
		ItemsList.Clear();
		_stats.Clear(); _prices.Clear(); _maxPrices.Clear();
		_cts.Cancel();
		_cts = new();

		ItemsList = await ItemSvc.SearchAsync(query);
		if (ItemsList.Count > 0)
			_ = LoadPricesAsync();
	}

	private async Task LoadPricesAsync()
	{
		foreach (var item in ItemsList)
		{
			if (_cts.IsCancellationRequested) break;
			var stat = await ItemSvc.GetStatisticAsync(item.Slug);
			_stats[item.Slug] = stat;
			_prices[item.Slug] = ItemSvc.GetReferencePrice(stat);
			_maxPrices[item.Slug] = ItemSvc.GetMaxReferencePrice(stat);
			StateHasChanged();
			await Task.Delay(250);
		}
	}

	protected string GetPrice(string slug)
	{
		if (_prices.TryGetValue(slug, out var p) && p.HasValue) return p.Value.ToString("F0");
		return _stats.ContainsKey(slug) ? "-" : "";
	}

	protected string GetMaxPrice(string slug)
	{
		if (_maxPrices.TryGetValue(slug, out var p) && p.HasValue) return p.Value.ToString("F0");
		return _stats.ContainsKey(slug) ? "-" : "";
	}

	public void Dispose() { _cts?.Cancel(); _cts?.Dispose(); }
}
