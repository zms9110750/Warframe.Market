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

	// ─── README ───
	protected string? readme;
	private static string? _readmeCache;

	protected override async Task OnInitializedAsync()
	{
		try
		{
			readme = _readmeCache ??= await System.IO.File.ReadAllTextAsync(
				System.IO.Path.Combine(AppContext.BaseDirectory, "README.md"));
		}
		catch { }
	}

	// ─── 搜索 ───

	protected async Task OnSearch(string query)
	{
		if (string.IsNullOrWhiteSpace(query)) return;

		var tabKey = query;
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
		await OnSearch(query);
	}

	private async Task DoSearchTab(string tab)
	{
		searchingTabs.Add(tab);
		var results = await ItemSvc.SearchAsync(tab);
		tabResults[tab] = results;
		searchingTabs.Remove(tab);
		StateHasChanged();
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

	public void Dispose() { }
}
