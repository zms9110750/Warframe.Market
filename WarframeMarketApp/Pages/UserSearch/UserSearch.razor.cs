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
		Log.Information("UserSearch 查询按钮点击: {Slug}", slug);

		var names = slug.Split('/', '\\', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		foreach (var name in names)
		{
			if (_pinnedUsers.Contains(name))
			{
				activeTabIndex = 1 + _pinnedUsers.IndexOf(name);
				Log.Information("UserSearch 切换到钉住: {Name}, index={Idx}", name, activeTabIndex);
				continue;
			}
			if (!_activeUsers.Contains(name))
			{
				_activeUsers.Add(name);
			}
			activeTabIndex = 1 + _pinnedUsers.Count + _activeUsers.IndexOf(name);
			Log.Information("UserSearch 添加标签: {Name}, index={Idx}", name, activeTabIndex);
			_ = SearchUserAsync(name, false);
		}
		Log.Information("UserSearch SearchAsync 完成");
		StateHasChanged();
	}

	private async Task SearchUserAsync(string name, bool isPinned)
	{
		if (_userResults.ContainsKey(name))
		{
			Log.Information("UserSearch 重复搜索跳过: {Name}", name);
			return;
		}
		_searchingUsers.Add(name);
		Log.Information("UserSearch 开始搜索用户: {Name} at {Time}", name, DateTime.Now);

		var result = new UserSearchResult { Loading = true };
		_userResults[name] = result;

		try
		{
			// 查用户
			Log.Information("UserSearch 请求用户信息: {Name}", name);
			var userResp = await Wfm.GetUserAsync(name);
			Log.Information("UserSearch 用户信息返回: {Name}, Data={HasData}", name, userResp?.Content?.Data != null);
			if (userResp?.Content?.Data == null)
			{
				result.NotFound = true;
				result.Loading = false;
				StateHasChanged();
				return;
			}
			result.User = userResp.Content.Data;

			// 查订单
			Log.Information("UserSearch 请求订单: {Name}", name);
			var orderResp = await Wfm.GetOrdersFromUserAsync(name);
			Log.Information("UserSearch 订单返回: {Name}, Count={Count}", name, orderResp?.Content?.Data?.Length ?? 0);
			if (orderResp?.Content?.Data == null || orderResp.Content.Data.Length == 0)
			{
				result.Loading = false;
				StateHasChanged();
				return;
			}
			result.Orders = orderResp.Content.Data.ToList();
			result.Loading = false;
			Log.Information("UserSearch 订单就绪: {Name}, {Count}条", name, result.Orders.Count);
			StateHasChanged();

			// 加载物品信息 + 价格
			// 注意：物品名称/I18n 从本地 SQLite 获取，不需要 API
			Log.Information("UserSearch 开始加载物品: {Name}, 共{Count}个", name, result.Orders.Count);
			result.LoadingPrices = true;

			// 从本地 Trie/SQLite 查找物品
			foreach (var o in result.Orders)
			{
				var itemId = o.ItemId ?? "";
				if (result.ItemCache.ContainsKey(itemId)) continue;
				var found = await ItemSvc.SearchAsync(itemId);
				var item = found.FirstOrDefault();
				if (item != null) result.ItemCache[itemId] = item;
			}
			Log.Information("UserSearch 物品加载完成: {Name}, 共{Count}个", name, result.ItemCache.Count);
			StateHasChanged();

			// 加载价格（用 ItemSvc 走缓存）
			Log.Information("UserSearch 开始加载价格: {Name}", name);
			var priceTasks = new List<Task>();
			foreach (var o in result.Orders)
			{
				var item = result.ItemCache.GetValueOrDefault(o.ItemId ?? "");
				if (item != null && !result.Prices.ContainsKey(item.Slug))
				{
					var slug = item.Slug;
					priceTasks.Add(LoadPriceAsync(result, slug));
					if (priceTasks.Count >= 3)
					{
						await Task.WhenAll(priceTasks);
						StateHasChanged();
						priceTasks.Clear();
					}
				}
			}
			await Task.WhenAll(priceTasks);
			StateHasChanged();
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
		Log.Information("UserSearch 搜索完全结束: {Name}", name);
	}

	private async Task<ItemShort?> LoadItemAsync(string itemId)
	{
		try
		{
			var resp = await Wfm.GetItemByIdAsync(itemId);
			return resp?.Content?.Data;
		}
		catch { return null; }
	}

	private async Task LoadPriceAsync(UserSearchResult result, string slug)
	{
		try
		{
			var stat = await ItemSvc.GetStatisticAsync(slug);
			if (stat != null) result.Prices[slug] = stat;
		}
		catch { }
	}

	protected void CloseUserTab(int idx)
	{
		if (idx < 0 || idx >= _activeUsers.Count) return;
		var name = _activeUsers[idx];
		_activeUsers.RemoveAt(idx);
		_userResults.Remove(name);
		_searchingUsers.Remove(name);
		Log.Information("UserSearch 关闭标签: {Name}", name);
	}

	protected void PinUser(string name) { _pinnedUsers.Add(name); Storage.PinUser(name); Log.Information("UserSearch 钉住: {Name}", name); }
	protected void UnpinUser(string name) { _pinnedUsers.Remove(name); Storage.UnpinUser(name); Log.Information("UserSearch 解绑: {Name}", name); }
}
