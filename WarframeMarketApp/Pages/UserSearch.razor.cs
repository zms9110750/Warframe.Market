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

public partial class UserSearch : ComponentBase, IDisposable
{
	[Inject] private ItemsService ItemSvc { get; set; } = null!;
	[Inject] private WarframeMarketClient Wfm { get; set; } = null!;

	protected string slug = "";
	protected User? user;
	protected List<Order>? orders;
	protected bool loading;
	protected bool searched;
	protected bool notFound;
	protected bool loadingPrices;
	protected string? error;
	private CancellationTokenSource _cts = new();

	protected List<DataTableHeader<Order>> _headers = new();

	// ItemId → ItemShort 缓存（用 API 查）
	private Dictionary<string, ItemShort?> _itemCache = new();
	// ItemId → Statistic 缓存
	private Dictionary<string, Statistic?> _prices = new();

	protected async Task SearchAsync()
	{
		Log.Information("UserSearch 查询: {User}", slug);
		if (string.IsNullOrWhiteSpace(slug)) return;
		if (_headers.Count == 0)
		{
			_headers.Add(new("物品", "item") { ValueExpression = (Func<Order, object?>)(o => GetItemName(o)) });
			_headers.Add(new("英文名称", "en") { ValueExpression = (Func<Order, object?>)(o => GetEnName(o)) });
			_headers.Add(new("类型", "Type") { Sortable = false });
			_headers.Add(new("铂金", nameof(Order.Platinum)));
			_headers.Add(new("数量", nameof(Order.Quantity)));
			_headers.Add(new("等级", nameof(Order.Rank)));
			_headers.Add(new("语言", "locale") { ValueExpression = (Func<Order, object?>)(o => GetLocale(o)) });
			_headers.Add(new("参考价", "ref") { Sortable = false });
			_headers.Add(new("差价", "diff") { Sortable = false });
		}
		loading = true; searched = true; notFound = false; error = null;
		user = null; orders = null;
		_itemCache.Clear(); _prices.Clear();
		_cts?.Cancel(); _cts = new();

		try
		{
			// 查用户
			var userResp = await Wfm.GetUserAsync(slug);
			if (userResp?.Content?.Data == null)
			{
				notFound = true;
				loading = false;
				return;
			}
			user = userResp.Content.Data;

			// 查订单
			var orderResp = await Wfm.GetOrdersFromUserAsync(slug);
			if (orderResp?.Content?.Data == null || orderResp.Content.Data.Length == 0)
			{
				searched = true;
				loading = false;
				StateHasChanged();
				return;
			}

			orders = orderResp.Content.Data.ToList();
			loading = false;
			StateHasChanged();

			// 异步加载物品信息和价格
			loadingPrices = true;
			_ = LoadItemInfoAsync(_cts.Token);
		}
		catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			notFound = true;
			loading = false;
		}
		catch (Exception ex)
		{
			error = ex.Message;
			loading = false;
		}
	}

	private async Task LoadItemInfoAsync(CancellationToken ct)
	{
		try
		{
			// Phase 1: 快速加载所有物品信息（让子面板可展开）
			foreach (var o in orders!)
			{
				if (ct.IsCancellationRequested) break;
				var itemId = o.ItemId ?? "";
				if (!_itemCache.ContainsKey(itemId))
				{
					try
					{
						var resp = await Wfm.GetItemByIdAsync(itemId, ct);
						if (resp?.Content?.Data != null)
							_itemCache[itemId] = resp.Content.Data;
					}
					catch { }
				}
			}
			StateHasChanged();

			// Phase 2: 逐条加载价格
			foreach (var o in orders!)
			{
				if (ct.IsCancellationRequested) break;
				var item = _itemCache.GetValueOrDefault(o.ItemId ?? "");
				if (item != null)
				{
					try
					{
						var stat = await ItemSvc.GetStatisticAsync(item.Slug, ct);
						_prices[item.Slug] = stat;
					}
					catch { }
				}
				StateHasChanged();
				await Task.Delay(100, ct);
			}
		}
		catch (OperationCanceledException) { }
		catch (Exception ex)
		{
			error = ex.Message;
		}
		loadingPrices = false;
		StateHasChanged();
	}

	// ─── 辅助方法 ───

	protected string GetItemName(Order o)
	{
		var item = _itemCache.GetValueOrDefault(o.ItemId ?? "");
		if (item == null) return "加载中...";

		return item.I18n.TryGetValue(Language.ZhHans, out var zh) ? zh.Name
			 : item.I18n.TryGetValue(Language.En, out var en) ? en.Name
			 : item.Slug;
	}

	protected string GetEnName(Order o)
	{
		var item = _itemCache.GetValueOrDefault(o.ItemId ?? "");
		if (item == null) return "";
		return item.I18n.TryGetValue(Language.En, out var en) ? en.Name : item.Slug;
	}

	protected string GetLocale(Order o)
	{
		return o.User?.Locale switch
		{
			"zh-hans" => "简体中文",
			"zh-hant" => "繁体中文",
			"en" => "英语",
			"ko" => "韩语",
			"ru" => "俄语",
			"de" => "德语",
			"fr" => "法语",
			"pt" => "葡萄牙语",
			"es" => "西班牙语",
			"it" => "意大利语",
			"pl" => "波兰语",
			"uk" => "乌克兰语",
			_ => o.User?.Locale ?? ""
		};
	}

	protected string GetRef(Order o)
	{
		var item = _itemCache.GetValueOrDefault(o.ItemId ?? "");
		if (item == null) return "";

		if (!_prices.TryGetValue(item.Slug, out var stat) || stat == null)
			return loadingPrices ? "" : "-";

		// 满级订单→满级价，否则→0级价
		double? p;
		if (o.Rank > 0)
			p = ItemSvc.GetMaxReferencePrice(stat);
		else
			p = ItemSvc.GetReferencePrice(stat);

		return p?.ToString("F0") ?? "-";
	}

	protected string GetDiff(Order o)
	{
		var item = _itemCache.GetValueOrDefault(o.ItemId ?? "");
		if (item == null) return "";

		if (!_prices.TryGetValue(item.Slug, out var stat) || stat == null) return "";
		double? refP;
		if (o.Rank > 0)
			refP = ItemSvc.GetMaxReferencePrice(stat);
		else
			refP = ItemSvc.GetReferencePrice(stat);

		if (refP == null || refP <= 0) return "";
		var diff = refP.Value - o.Platinum;
		return diff >= 0 ? $"+{diff:F0}" : $"{diff:F0}";
	}

	protected ItemShort? GetItemShort(Order o)
	{
		return _itemCache.GetValueOrDefault(o.ItemId ?? "");
	}

	public void Dispose()
	{
		_cts?.Cancel();
		_cts?.Dispose();
	}
}
