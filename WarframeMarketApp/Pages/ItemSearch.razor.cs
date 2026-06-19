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

	// activeTabIndex: 0=搜索标签, 1+=钉住+临时标签
	protected int activeTabIndex;

	// 临时搜索标签（非钉住的搜索结果）
	protected List<string> activeTabs = new();
	protected HashSet<string> searchingTabs = new();

	// 钉住的搜索词
	protected List<string> _pinnedSearches = new();

	// 每个标签下，每个 / 段各自的结果
	protected Dictionary<string, List<List<ItemShort>>> tabTermResults = new();
	protected Dictionary<string, List<string>> tabTerms = new();

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

		// 加载钉住的搜索
		_pinnedSearches = Storage.Load().PinnedSearches.ToList();
		foreach (var q in _pinnedSearches)
			await DoSearchTab(q);
	}

	protected async Task OnSearch(string query)
	{
		Log.Information("ItemSearch 搜索: {Query}", query);
		if (string.IsNullOrWhiteSpace(query)) return;

		// 如果是钉住的，只切换标签，不重复搜索
		if (_pinnedSearches.Contains(query))
		{
			activeTabIndex = 1 + _pinnedSearches.IndexOf(query);
			return;
		}

		if (!activeTabs.Contains(query))
		{
			activeTabs.Add(query);
		}
		activeTabIndex = 1 + _pinnedSearches.Count + activeTabs.IndexOf(query);

		if (!tabTermResults.ContainsKey(query))
			await DoSearchTab(query);
	}

	protected async Task OnFavoriteClick(string query)
	{
		await OnSearch(query);
	}

	private async Task DoSearchTab(string tab)
	{
		if (tabTermResults.ContainsKey(tab)) return; // 已加载

		searchingTabs.Add(tab);
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
		if (idx < 0 || idx >= activeTabs.Count) return;
		var tab = activeTabs[idx];
		if (_pinnedSearches.Contains(tab)) return;
		activeTabs.RemoveAt(idx);
		tabTermResults.Remove(tab);
		tabTerms.Remove(tab);
		searchingTabs.Remove(tab);
		if (activeTabIndex >= 1 + _pinnedSearches.Count + activeTabs.Count)
			activeTabIndex = Math.Max(0, 1 + _pinnedSearches.Count + activeTabs.Count - 1);
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
		if (activeTabIndex > 1 + _pinnedSearches.Count)
			activeTabIndex--;
	}
}
