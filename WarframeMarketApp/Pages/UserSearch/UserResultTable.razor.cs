using Microsoft.AspNetCore.Components;
using Masa.Blazor;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Orders;
using zms9110750.WarframeMarketApi.Models.Statistics;
using WarframeMarketApp.Services;

namespace WarframeMarketApp.Pages.UserSearch;

public partial class UserResultTable : ComponentBase
{
	[Inject] private ItemsService ItemSvc { get; set; } = null!;

	[Parameter] public string UserName { get; set; } = "";
	[Parameter] public UserSearchResult? result { get; set; }

	protected List<DataTableHeader<Order>> _headers = new()
	{
		new("物品", nameof(Order.Id)) { Sortable = false },
		new("英文名称", nameof(Order.Id)) { Sortable = false },
		new("类型", nameof(Order.Type)) { Sortable = false },
		new("铂金", nameof(Order.Platinum)),
		new("数量", nameof(Order.Quantity)),
		new("等级", nameof(Order.Rank)),
		new("参考价", nameof(Order.Id)) { Sortable = false },
		new("差价", nameof(Order.Id)) { Sortable = false },
	};

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
}
