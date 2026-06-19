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

namespace WarframeMarketApp.Pages.UserSearch;

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
	private CancellationTokenSource _cts = new();

	protected override void OnInitialized()
	{
		Log.Information("UserSearch 初始化");
		_pinnedUsers = Storage.Load().PinnedUsers.ToList();
		foreach (var name in _pinnedUsers)
			_ = SearchUserAsync(name, true);
	}

	protected async Task SearchAsync()
	{
		if (string.IsNullOrWhiteSpace(slug)) return;
		Log.Information("UserSearch 查询: {Slug}", slug);

		var names = slug.Split('/', '\\', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		foreach (var name in names)
		{
			if (_pinnedUsers.Contains(name))
			{
				activeTabIndex = 1 + _pinnedUsers.IndexOf(name);
				continue;
			}
			if (!_activeUsers.Contains(name))
			{
				_activeUsers.Add(name);
			}
			activeTabIndex = 1 + _pinnedUsers.Count + _activeUsers.IndexOf(name);
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
			int itemOk = 0, itemFail = 0;
			foreach (var o in result.Orders)
			{
				var itemId = o.ItemId ?? "";
				if (!result.ItemCache.ContainsKey(itemId))
				{
					try
					{
						var resp = await Wfm.GetItemByIdAsync(itemId);
						if (resp?.Content?.Data != null)
						{
							result.ItemCache[itemId] = resp.Content.Data;
							itemOk++;
						}
						else itemFail++;
					}
					catch (Exception ex) { itemFail++; if (itemFail <= 3) Log.Error(ex, "GetItemByIdAsync失败 {Id}", itemId); }
				}
			}
			Log.Information("UserSearch 物品加载: {Name}, 成功={Ok}, 失败={Fail}", name, itemOk, itemFail);
			StateHasChanged();

			// 加载价格
			result.LoadingPrices = true;
			int priceCount = 0;
			int failCount = 0;
			foreach (var o in result.Orders)
			{
				var item = result.ItemCache.GetValueOrDefault(o.ItemId ?? "");
				if (item != null && !result.Prices.ContainsKey(item.Slug))
				{
					try
					{
						var stat = await ItemSvc.GetStatisticAsync(item.Slug);
						if (stat != null)
						{
							result.Prices[item.Slug] = stat;
							priceCount++;
						}
						else
						{
							failCount++;
							if (failCount <= 3) // 只记前3个失败的
								Log.Warning("统计返回null: {Slug}", item.Slug);
						}
					}
					catch (Exception ex2) { Log.Error(ex2, "价格加载失败 {Slug}", item.Slug); failCount++; }
				}
				await Task.Delay(200); // 每秒约5个请求
			}
			Log.Information("UserSearch 价格加载完成: {Name}, 成功={Count}, 失败={Fail}/{Total}", name, priceCount, failCount, result.Orders.Count);
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
