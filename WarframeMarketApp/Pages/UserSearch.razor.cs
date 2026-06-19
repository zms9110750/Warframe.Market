using Microsoft.AspNetCore.Components;
using Masa.Blazor;
using Serilog;
using System.Net.Http;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Orders;
using zms9110750.WarframeMarketApi.Models.Users;
using zms9110750.WarframeMarketApi.Models.Statistics;
using WarframeMarketApp.Services;

namespace WarframeMarketApp.Pages;

public partial class UserSearch : ComponentBase
{
	[Inject] private ItemsService ItemSvc { get; set; } = null!;
	[Inject] private WarframeMarketClient Wfm { get; set; } = null!;
	[Inject] private PersistentStorage Storage { get; set; } = null!;
	[CascadingParameter(Name = "CanWrite")] public bool canWrite { get; set; }

	protected string slug = "";
	protected int activeTabIndex;

	protected List<string> _pinnedUsers = new();
	protected List<string> _activeUsers = new();
	protected HashSet<string> _searchingUsers = new();
	protected Dictionary<string, UserSearchResult> _userResults = new();

	protected List<DataTableHeader<Order>> _headers = new();
	private CancellationTokenSource _cts = new();

	protected override void OnInitialized()
	{
		Log.Information("UserSearch 初始化");
		_pinnedUsers = Storage.Load().PinnedUsers.ToList();
		foreach (var name in _pinnedUsers)
			_ = SearchUserAsync(name, true);

		if (_headers.Count == 0)
		{
			_headers.Add(new("物品", nameof(Order.Id)) { Sortable = false });
			_headers.Add(new("英文名称", nameof(Order.Id)) { Sortable = false });
			_headers.Add(new("类型", nameof(Order.Type)) { Sortable = false });
			_headers.Add(new("铂金", nameof(Order.Platinum)));
			_headers.Add(new("数量", nameof(Order.Quantity)));
			_headers.Add(new("等级", nameof(Order.Rank)));
			_headers.Add(new("参考价", nameof(Order.Id)) { Sortable = false });
			_headers.Add(new("差价", nameof(Order.Id)) { Sortable = false });
		}
	}

	protected async Task SearchAsync()
	{
		if (string.IsNullOrWhiteSpace(slug)) return;
		Log.Information("UserSearch 查询: {Slug}", slug);

		var names = slug.Split('/', '\\', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		foreach (var name in names)
		{
			if (!_activeUsers.Contains(name) && !_pinnedUsers.Contains(name))
			{
				_activeUsers.Add(name);
				activeTabIndex = 1 + _pinnedUsers.Count + _activeUsers.Count - 1;
			}
			_ = SearchUserAsync(name, false);
		}
		StateHasChanged();
	}

	private async Task SearchUserAsync(string name, bool isPinned)
	{
		if (_userResults.ContainsKey(name)) return;
		_searchingUsers.Add(name);

		var result = new UserSearchResult { Loading = true };
		_userResults[name] = result;

		try
		{
			var userResp = await Wfm.GetUserAsync(name);
			if (userResp?.Content?.Data == null)
			{
				result.NotFound = true;
				result.Loading = false;
				StateHasChanged();
				return;
			}
			result.User = userResp.Content.Data;

			var orderResp = await Wfm.GetOrdersFromUserAsync(name);
			if (orderResp?.Content?.Data == null || orderResp.Content.Data.Length == 0)
			{
				result.Loading = false;
				StateHasChanged();
				return;
			}
			result.Orders = orderResp.Content.Data.ToList();
			result.Loading = false;
			StateHasChanged();

			// 加载物品信息
			foreach (var o in result.Orders)
			{
				var itemId = o.ItemId ?? "";
				if (!result.ItemCache.ContainsKey(itemId))
				{
					try
					{
						var resp = await Wfm.GetItemByIdAsync(itemId);
						if (resp?.Content?.Data != null)
							result.ItemCache[itemId] = resp.Content.Data;
					}
					catch { }
				}
			}
			StateHasChanged();

			// 加载价格
			result.LoadingPrices = true;
			int priceCount = 0;
			foreach (var o in result.Orders)
			{
				var item = result.ItemCache.GetValueOrDefault(o.ItemId ?? "");
				if (item != null && !result.Prices.ContainsKey(item.Slug))
				{
					try
					{
						var stat = await ItemSvc.GetStatisticAsync(item.Slug);
						result.Prices[item.Slug] = stat;
						if (stat != null) priceCount++;
					}
					catch (Exception ex2) { Log.Error(ex2, "价格加载失败 {Slug}", item.Slug); }
				}
				await Task.Delay(100);
			}
			Log.Information("UserSearch 价格加载完成: {Name}, {Count}/{Total}", name, priceCount, result.Orders.Count);
			result.LoadingPrices = false;
			StateHasChanged();
		}
		catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			Log.Information("UserSearch 用户不存在: {Name}", name);
			result.NotFound = true;
		}
		catch (Exception ex)
		{
			Log.Error(ex, "UserSearch 查询失败: {Name}", name);
			result.Error = ex.Message;
		}
		result.Loading = false;
		_searchingUsers.Remove(name);
		StateHasChanged();
	}

	// ─── 辅助方法 ───
	protected string GetItemName(UserSearchResult r, Order o)
	{
		var item = r.ItemCache.GetValueOrDefault(o.ItemId ?? "");
		if (item == null) return "加载中...";
		return item.I18n.TryGetValue(Language.ZhHans, out var zh) ? zh.Name
			 : item.I18n.TryGetValue(Language.En, out var en) ? en.Name
			 : item.Slug;
	}
	protected string GetEnName(UserSearchResult r, Order o)
	{
		var item = r.ItemCache.GetValueOrDefault(o.ItemId ?? "");
		return item?.I18n.TryGetValue(Language.En, out var en) == true ? en.Name : "";
	}
	protected string GetLocale(UserSearchResult r, Order o)
	{
		return ""; // 不需要语言列
	}
	protected string GetRef(UserSearchResult r, Order o)
	{
		var item = r.ItemCache.GetValueOrDefault(o.ItemId ?? "");
		if (item == null || !r.Prices.TryGetValue(item.Slug, out var stat) || stat == null) return "-";
		var p = o.Rank > 0 ? ItemSvc.GetMaxReferencePrice(stat) : ItemSvc.GetReferencePrice(stat);
		return p?.ToString("F0") ?? "-";
	}
	protected string GetDiff(UserSearchResult r, Order o)
	{
		var item = r.ItemCache.GetValueOrDefault(o.ItemId ?? "");
		if (item == null || !r.Prices.TryGetValue(item.Slug, out var stat) || stat == null) return "";
		var refP = o.Rank > 0 ? ItemSvc.GetMaxReferencePrice(stat) : ItemSvc.GetReferencePrice(stat);
		if (refP == null || refP <= 0) return "";
		var diff = refP.Value - o.Platinum;
		return diff >= 0 ? $"+{diff:F0}" : $"{diff:F0}";
	}
	protected ItemShort? GetItemShort(UserSearchResult r, Order o)
	{
		return r.ItemCache.GetValueOrDefault(o.ItemId ?? "");
	}

	protected void CloseUserTab(int idx)
	{
		if (idx < 0 || idx >= _activeUsers.Count) return;
		var name = _activeUsers[idx];
		_activeUsers.RemoveAt(idx);
		_userResults.Remove(name);
		_searchingUsers.Remove(name);
	}

	protected void PinUser(string name) { _pinnedUsers.Add(name); Storage.PinUser(name); }
	protected void UnpinUser(string name) { _pinnedUsers.Remove(name); Storage.UnpinUser(name); }
}
