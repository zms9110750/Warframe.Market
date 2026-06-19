using Microsoft.AspNetCore.Components;
using Masa.Blazor;
using Serilog;
using zms9110750.WarframeMarketApi.Models.Items;
using WarframeMarketApp.Services;

namespace WarframeMarketApp.Pages;

public partial class ItemSearch : ComponentBase, IDisposable
{
	[Inject] private ItemsService ItemSvc { get; set; } = null!;

	protected List<string> activeTabs = new();
	protected HashSet<string> searchingTabs = new();
	protected int activeTabIndex;
	protected Dictionary<string, List<ItemShort>> tabResults = new();
	protected string? readme;
	private static string? _readmeCache;

	protected override async Task OnInitializedAsync()
	{
		Log.Information("ItemSearch 初始化");
		try
		{
			readme = _readmeCache ??= await System.IO.File.ReadAllTextAsync(
				System.IO.Path.Combine(AppContext.BaseDirectory, "README.md"));
		}
		catch { }
	}

	protected async Task OnSearch(string query)
	{
		Log.Information("ItemSearch 搜索: {Query}", query);
		if (string.IsNullOrWhiteSpace(query)) return;

		var tabKey = query;
		if (!activeTabs.Contains(tabKey))
		{
			activeTabs.Add(tabKey);
			activeTabIndex = activeTabs.Count - 1;
		}
		else
			activeTabIndex = activeTabs.IndexOf(tabKey);

		await DoSearchTab(tabKey);
	}

	protected async Task OnFavoriteClick(string query)
	{
		Log.Information("ItemSearch 收藏点击: {Query}", query);
		await OnSearch(query);
	}

	private async Task DoSearchTab(string tab)
	{
		searchingTabs.Add(tab);
		var results = await ItemSvc.SearchAsync(tab);
		tabResults[tab] = results;
		searchingTabs.Remove(tab);
		Log.Information("ItemSearch 标签 {Tab}: {Count} 结果", tab, results.Count);
		StateHasChanged();
	}

	protected void CloseTab(int idx)
	{
		Log.Information("ItemSearch 关闭标签 {Idx}", idx);
		if (idx < 0 || idx >= activeTabs.Count) return;
		var tab = activeTabs[idx];
		activeTabs.RemoveAt(idx);
		tabResults.Remove(tab);
		searchingTabs.Remove(tab);
		if (activeTabIndex >= activeTabs.Count)
			activeTabIndex = Math.Max(0, activeTabs.Count - 1);
	}

	public void Dispose() { }
}
