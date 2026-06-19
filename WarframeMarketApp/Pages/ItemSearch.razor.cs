using Microsoft.AspNetCore.Components;
using Serilog;
using zms9110750.WarframeMarketApi.Models.Items;
using WarframeMarketApp.Services;

namespace WarframeMarketApp.Pages;

public partial class ItemSearch : ComponentBase
{
	[Inject] private ItemsService ItemSvc { get; set; } = null!;
	[Inject] private PersistentStorage Storage { get; set; } = null!;
	[CascadingParameter(Name = "CanWrite")] public bool canWrite { get; set; }

	// 钉住搜索词

	// 每个标签对应一个完整查询词（含 /）
	protected List<string> activeTabs = new();
	protected HashSet<string> searchingTabs = new();
	protected int activeTabIndex;

	// 每个标签下，每个 / 段各自的结果
	protected Dictionary<string, List<List<ItemShort>>> tabTermResults = new();
	protected Dictionary<string, List<string>> tabTerms = new(); // "盲怒/wisp" → ["盲怒", "wisp"]

	protected string? readme;
	private static string? _readmeCache;

	// 钉住的搜索
	protected HashSet<string> _pinnedSearches = new();

	protected override async Task OnInitializedAsync()
	{
		Log.Information("ItemSearch 初始化");
		try
		{
			readme = _readmeCache ??= await System.IO.File.ReadAllTextAsync(
				System.IO.Path.Combine(AppContext.BaseDirectory, "README.md"));
		}
		catch { }

		// 加载钉住的搜索
		_pinnedSearches = new(Storage.Load().PinnedSearches);
		foreach (var q in _pinnedSearches)
			await OnSearch(q);
	}

	protected async Task OnSearch(string query)
	{
		Log.Information("ItemSearch 搜索: {Query}", query);
		if (string.IsNullOrWhiteSpace(query)) return;

		if (!activeTabs.Contains(query))
		{
			activeTabs.Add(query);
			activeTabIndex = activeTabs.Count - 1;
		}
		else
			activeTabIndex = activeTabs.IndexOf(query);

		await DoSearchTab(query);
	}

	protected async Task OnFavoriteClick(string query)
	{
		await OnSearch(query);
	}

	private async Task DoSearchTab(string tab)
	{
		searchingTabs.Add(tab);

		// 按 / 分隔为多个词
		var terms = tab.Split('/', '\\', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
		tabTerms[tab] = terms;

		var allResults = new List<List<ItemShort>>();
		foreach (var term in terms)
		{
			var results = await ItemSvc.SearchAsync(term);
			allResults.Add(results);
		}
		tabTermResults[tab] = allResults;

		searchingTabs.Remove(tab);
		Log.Information("ItemSearch 标签 {Tab}: {Terms} 个词", tab, terms.Count);
		StateHasChanged();
	}

	protected void CloseTab(int idx)
	{
		Log.Information("ItemSearch 关闭标签 {Idx}", idx);
		if (idx < 0 || idx >= activeTabs.Count) return;
		var tab = activeTabs[idx];
		if (_pinnedSearches.Contains(tab)) return; // 钉住的不让关
		activeTabs.RemoveAt(idx);
		tabTermResults.Remove(tab);
		tabTerms.Remove(tab);
		searchingTabs.Remove(tab);
		if (activeTabIndex >= activeTabs.Count)
			activeTabIndex = Math.Max(0, activeTabs.Count - 1);
	}

	protected void PinTab(string tab)
	{
		_pinnedSearches.Add(tab);
		Storage.PinSearch(tab);
	}

	protected void UnpinTab(string tab)
	{
		_pinnedSearches.Remove(tab);
		Storage.UnpinSearch(tab);
	}
}
