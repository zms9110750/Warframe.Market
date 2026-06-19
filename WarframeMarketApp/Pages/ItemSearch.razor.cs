using Microsoft.AspNetCore.Components;
using Masa.Blazor;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Statistics;
using WarframeMarketApp.Services;

namespace WarframeMarketApp.Pages;

public partial class ItemSearch : ComponentBase, IDisposable
{
	[Inject] private ItemsService ItemSvc { get; set; } = null!;

	// ─── 搜索标签 ───
	protected List<string> activeTabs = new();
	protected HashSet<string> searchingTabs = new();
	protected int activeTabIndex;

	// ─── 结果 ───
	protected Dictionary<string, List<ItemShort>> tabResults = new();
	protected HashSet<(string Tab, string Id)> hiddenResults = new();

	// ─── README ───
	protected string? readme;

	// ─── 统计数据缓存 ───
	private CancellationTokenSource _cts = new();
	private Dictionary<string, Statistic?> _stats = new();
	internal Dictionary<string, double?> Prices = new();
	internal Dictionary<string, double?> MaxPrices = new();

	protected override async Task OnInitializedAsync()
	{
		// 尝试加载 README
		try
		{
			readme = _readmeCache ??= await System.IO.File.ReadAllTextAsync(
				System.IO.Path.Combine(AppContext.BaseDirectory, "README.md"));
		}
		catch { }
	}
	private static string? _readmeCache;

	// ─── 搜索 ───

	protected async Task OnSearch(string query)
	{
		if (string.IsNullOrWhiteSpace(query)) return;

		// 开新标签
		var tabKey = $"{query}";
		if (!activeTabs.Contains(tabKey))
		{
			activeTabs.Add(tabKey);
			activeTabIndex = activeTabs.Count - 1;
		}
		else
		{
			activeTabIndex = activeTabs.IndexOf(tabKey);
		}

		await DoSearchTab(tabKey);
	}

	protected async Task OnFavoriteClick(string query)
	{
		// 和 OnSearch 一样，打开标签
		await OnSearch(query);
	}

	private async Task DoSearchTab(string tab)
	{
		searchingTabs.Add(tab);
		_cts.Cancel();
		_cts = new();

		var results = await ItemSvc.SearchAsync(tab);
		tabResults[tab] = results;
		searchingTabs.Remove(tab);

		// 异步加载价格
		if (results.Count > 0)
			_ = LoadPricesForTabAsync(tab, results, _cts.Token);

		StateHasChanged();
	}

	private async Task LoadPricesForTabAsync(string tab, List<ItemShort> items, CancellationToken ct)
	{
		foreach (var item in items)
		{
			if (ct.IsCancellationRequested) break;
			var stat = await ItemSvc.GetStatisticAsync(item.Slug, ct);
			if (stat == null) continue;
			_stats[item.Slug] = stat;
			Prices[item.Slug] = ItemSvc.GetReferencePrice(stat);
			MaxPrices[item.Slug] = ItemSvc.GetMaxReferencePrice(stat);
			StateHasChanged();
			await Task.Delay(100, ct);
		}
	}

	// ─── 标签操作 ───

	protected void CloseTab(int idx)
	{
		if (idx < 0 || idx >= activeTabs.Count) return;
		var tab = activeTabs[idx];
		activeTabs.RemoveAt(idx);
		tabResults.Remove(tab);
		searchingTabs.Remove(tab);

		if (activeTabIndex >= activeTabs.Count)
			activeTabIndex = Math.Max(0, activeTabs.Count - 1);
	}

	// ─── 折叠 ───

	protected void ToggleHide(string tab, string itemId)
	{
		var key = (tab, itemId);
		if (!hiddenResults.Add(key))
			hiddenResults.Remove(key);
	}

	// ─── 价格查询（供子组件用） ───


	public void Dispose()
	{
		_cts.Cancel();
		_cts.Dispose();
	}
}
