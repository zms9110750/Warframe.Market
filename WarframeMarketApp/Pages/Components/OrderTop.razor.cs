using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Masa.Blazor;
using Serilog;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Orders;

namespace WarframeMarketApp.Pages.Components;

public partial class OrderTop : ComponentBase, IDisposable
{
	[Inject] private WarframeMarketClient Wfm { get; set; } = null!;
	[Inject] private IJSRuntime Js { get; set; } = null!;

	[CascadingParameter(Name = "ClickLink")] public bool ClickLink { get; set; }
	[Parameter] public ItemShort? TargetItem { get; set; }

	protected bool loading = true;
	protected bool _showBuy = false; // 默认售
	protected bool _showMaxRank; // 默认关
	protected string _maxRankLabel = "满级";
	private bool _previousShowMaxRank;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (_previousShowMaxRank != _showMaxRank)
		{
			_previousShowMaxRank = _showMaxRank;
			await RefreshWithRankAsync();
			StateHasChanged();
		}
	}

	protected Order[] TopOrder = Array.Empty<Order>();

	protected IEnumerable<Order> FilteredOrders
	{
		get
		{
			// 满级开关切换时已重新请求 API，这里只做购/售过滤
			var q = TopOrder.AsEnumerable();
			q = q.Where(o => _showBuy ? (o.Type is "buy" or "Buy") : (o.Type is "sell" or "Sell"));
			return q;
		}
	}
	protected List<DataTableHeader<Order>> _headers = new()
	{
		new("联系", nameof(Order.Id)) { Sortable = false },
		new("卖家", nameof(Order.User)) { ValueExpression = (Func<Order, object?>)(r => r.User?.IngameName) },
		new("声誉", nameof(Order.User)) { ValueExpression = (Func<Order, object?>)(r => r.User?.Reputation) },
		new("语言", nameof(Order.User)) { ValueExpression = (Func<Order, object?>)(r => r.User?.Locale) },
		new("价格", nameof(Order.Platinum)),
		new("数量", nameof(Order.Quantity)),
	};

	protected override async Task OnInitializedAsync()
	{
		try
		{
			if (TargetItem == null) { Log.Warning("OrderTop TargetItem 为 null"); loading = false; return; }
			Log.Information("OrderTop 初始化: {Slug}", TargetItem.Slug);
			loading = true;

			// 默认开关：MOD/赋能→满级，遗物→光辉，组件→成品
			var tags = TargetItem.Tags ?? new();
			if (tags.Contains("mod") || tags.Contains("arcane_enhancement"))
			{
				_showMaxRank = true;
				_maxRankLabel = "满级";
			}
			else if (tags.Contains("relic"))
			{
				_showMaxRank = true;
				_maxRankLabel = "光辉";
			}
			else if (tags.Contains("component"))
			{
				_showMaxRank = true;
				_maxRankLabel = "成品";
			}
			else if (tags.Contains("ayatan_sculpture"))
			{
				_showMaxRank = true;
				_maxRankLabel = "满星";
			}

			var orders = new List<Order>();
			HashSet<string> Tags = TargetItem.Tags ?? new();
			var slug = TargetItem.Slug;
			var maxRank = TargetItem.MaxRank;
			var maxAmber = TargetItem.MaxAmberStars;
			var maxCyan = TargetItem.MaxCyanStars;

			var itemType =
				Tags.Contains("riven") ? "riven" :
				Tags.Contains("mod") ? "mod" :
				Tags.Contains("arcane_enhancement") ? "arcane" :
				Tags.Contains("relic") ? "relic" :
				Tags.Contains("ayatan_sculpture") ? "ayatan" :
				Tags.Contains("component") ? "component" : null;

			switch (itemType)
			{
				case "arcane":
				case "mod":
				{
					var a = await Wfm.GetOrdersItemTopAsync(slug, new(RankLt: (maxRank ?? 1) - 1, Rank: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null, Subtype: null));
					if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					a = await Wfm.GetOrdersItemTopAsync(slug, new(Rank: maxRank, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null, Subtype: null));
					if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					_headers.Add(new("等级", nameof(Order.Rank)));
					break;
				}
				case "ayatan":
				{
					var a = await Wfm.GetOrdersItemTopAsync(slug, new(Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStarsLt: (maxAmber ?? 1) - 1, AmberStars: null, CyanStarsLt: (maxCyan ?? 1) - 1, CyanStars: null, Subtype: null));
					if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					a = await Wfm.GetOrdersItemTopAsync(slug, new(AmberStars: maxAmber, CyanStars: maxCyan, Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStarsLt: null, CyanStarsLt: null, Subtype: null));
					if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					_headers.Add(new("琥珀星", nameof(Order.AmberStars)));
					_headers.Add(new("青蓝星", nameof(Order.CyanStars)));
					break;
				}
				case "component":
				{
					var a = await Wfm.GetOrdersItemTopAsync(slug, new(Subtype: "blueprint", Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null));
					if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					a = await Wfm.GetOrdersItemTopAsync(slug, new(Subtype: "crafted", Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null));
					if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					_headers.Add(new("类型", nameof(Order.Subtype)));
					break;
				}
				case "relic":
				{
					var a = await Wfm.GetOrdersItemTopAsync(slug, new(Subtype: "intact", Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null));
					if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					a = await Wfm.GetOrdersItemTopAsync(slug, new(Subtype: "radiant", Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null));
					if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					_headers.Add(new("类型", nameof(Order.Subtype)));
					break;
				}
				case "riven":
				{
					var a = await Wfm.GetOrdersItemTopAsync(slug, new(Subtype: "revealed", Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null));
					if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					a = await Wfm.GetOrdersItemTopAsync(slug, new(Subtype: "unrevealed", Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null));
					if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					_headers.Add(new("类型", nameof(Order.Subtype)));
					break;
				}
				default:
				{
					var a = await Wfm.GetOrdersItemTopAsync(slug, null);
					if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					break;
				}
			}

			TopOrder = orders.Distinct().ToArray();
		}
		catch (Exception ex) { Log.Error(ex, "OrderTop 加载失败"); }
		finally { loading = false; }
	}

	/// <summary>满级开关变化时重新请求 API</summary>
	private async Task RefreshWithRankAsync()
	{
		if (TargetItem == null) return;
		try
		{
			loading = true;
			StateHasChanged();

			var orders = new List<Order>();
			HashSet<string> Tags = TargetItem.Tags ?? new();
			var slug = TargetItem.Slug;
			var maxRank = TargetItem.MaxRank;
			var maxAmber = TargetItem.MaxAmberStars;
			var maxCyan = TargetItem.MaxCyanStars;

			var isMaxRank = _showMaxRank;

			var itemType =
				Tags.Contains("riven") ? "riven" :
				Tags.Contains("mod") ? "mod" :
				Tags.Contains("arcane_enhancement") ? "arcane" :
				Tags.Contains("relic") ? "relic" :
				Tags.Contains("ayatan_sculpture") ? "ayatan" :
				Tags.Contains("component") ? "component" : null;

			var query = isMaxRank
				? new OrderTopQueryParameter(Rank: maxRank, RankLt: null, Charges: null, ChargesLt: null,
					AmberStars: maxAmber, AmberStarsLt: null, CyanStars: maxCyan, CyanStarsLt: null, Subtype: null)
				: null;

			switch (itemType)
			{
				case "arcane":
				case "mod":
				{
					if (isMaxRank)
					{
						var a = await Wfm.GetOrdersItemTopAsync(slug, query);
						if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					}
					else
					{
						var a = await Wfm.GetOrdersItemTopAsync(slug, null);
						if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					}
					break;
				}
				case "relic":
				{
					var subtype = isMaxRank ? "radiant" : null;
					var a = await Wfm.GetOrdersItemTopAsync(slug, subtype != null
						? new OrderTopQueryParameter(Subtype: subtype, Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null)
						: null);
					if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					break;
				}
				case "component":
				{
					var subtype = isMaxRank ? "crafted" : null;
					var a = await Wfm.GetOrdersItemTopAsync(slug, subtype != null
						? new OrderTopQueryParameter(Subtype: subtype, Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null)
						: null);
					if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					break;
				}
				case "ayatan":
				{
					if (isMaxRank)
					{
						var a = await Wfm.GetOrdersItemTopAsync(slug, new OrderTopQueryParameter(AmberStars: maxAmber, CyanStars: maxCyan,
							Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStarsLt: null, CyanStarsLt: null, Subtype: null));
						if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					}
					else
					{
						var a = await Wfm.GetOrdersItemTopAsync(slug, null);
						if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					}
					break;
				}
				default:
				{
					var a = await Wfm.GetOrdersItemTopAsync(slug, null);
					if (a?.Content?.Data != null) orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
					break;
				}
			}

			TopOrder = orders.Distinct().ToArray();
		}
		catch (Exception ex) { Log.Error(ex, "OrderTop 刷新失败"); }
		finally { loading = false; StateHasChanged(); }
	}

	protected async Task CopyAsync(string text)
	{
		try { await Js.InvokeVoidAsync("navigator.clipboard.writeText", text); }
		catch { }
	}

	public void Dispose() { }
}
