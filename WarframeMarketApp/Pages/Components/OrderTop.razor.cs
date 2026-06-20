using Masa.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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
    protected bool _showBuy = false;
    protected int _selectedRank = 0;
    protected int _maxRankValue = 0;
    private int _refreshKey;

    protected Order[] TopOrder = Array.Empty<Order>();

    protected IEnumerable<Order> FilteredOrders
    {
        get
        {
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

            _maxRankValue = TargetItem.MaxRank ?? 0;
            var slug = TargetItem.Slug;
            var maxRank = TargetItem.MaxRank;
            var maxAmber = TargetItem.MaxAmberStars;
            var maxCyan = TargetItem.MaxCyanStars;
            var s = TargetItem.Subtypes ?? FallbackSubtypes(TargetItem.Tags);

            var orders = new List<Order>();

            switch (s)
            {
                case { IsMod: true } or { IsArcane: true }:
                {
                    var a = await Wfm.GetOrdersItemTopAsync(slug, new(RankLt: (maxRank ?? 1) - 1, Rank: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null, Subtype: null));
                    if (a?.Content?.Data != null)
                    {
                        orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
                    }

                    a = await Wfm.GetOrdersItemTopAsync(slug, new(Rank: maxRank, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null, Subtype: null));
                    if (a?.Content?.Data != null)
                    {
                        orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
                    }

                    _headers.Add(new("等级", nameof(Order.Rank)));
                    break;
                }
                case { IsAyatan: true }:
                {
                    var a = await Wfm.GetOrdersItemTopAsync(slug, new(Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStarsLt: (maxAmber ?? 1) - 1, AmberStars: null, CyanStarsLt: (maxCyan ?? 1) - 1, CyanStars: null, Subtype: null));
                    if (a?.Content?.Data != null)
                    {
                        orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
                    }

                    a = await Wfm.GetOrdersItemTopAsync(slug, new(AmberStars: maxAmber, CyanStars: maxCyan, Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStarsLt: null, CyanStarsLt: null, Subtype: null));
                    if (a?.Content?.Data != null)
                    {
                        orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
                    }

                    _headers.Add(new("琥珀星", nameof(Order.AmberStars)));
                    _headers.Add(new("青蓝星", nameof(Order.CyanStars)));
                    break;
                }
                case { IsComponent: true }:
                {
                    var a = await Wfm.GetOrdersItemTopAsync(slug, new(Subtype: "blueprint", Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null));
                    if (a?.Content?.Data != null)
                    {
                        orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
                    }

                    a = await Wfm.GetOrdersItemTopAsync(slug, new(Subtype: "crafted", Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null));
                    if (a?.Content?.Data != null)
                    {
                        orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
                    }

                    _headers.Add(new("类型", nameof(Order.Subtype)));
                    break;
                }
                case { IsRelic: true }:
                {
                    var a = await Wfm.GetOrdersItemTopAsync(slug, new(Subtype: "intact", Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null));
                    if (a?.Content?.Data != null)
                    {
                        orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
                    }

                    a = await Wfm.GetOrdersItemTopAsync(slug, new(Subtype: "radiant", Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null));
                    if (a?.Content?.Data != null)
                    {
                        orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
                    }

                    _headers.Add(new("类型", nameof(Order.Subtype)));
                    break;
                }
                case { IsRiven: true }:
                {
                    var a = await Wfm.GetOrdersItemTopAsync(slug, new(Subtype: "revealed", Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null));
                    if (a?.Content?.Data != null)
                    {
                        orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
                    }

                    a = await Wfm.GetOrdersItemTopAsync(slug, new(Subtype: "unrevealed", Rank: null, RankLt: null, Charges: null, ChargesLt: null, AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null));
                    if (a?.Content?.Data != null)
                    {
                        orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
                    }

                    _headers.Add(new("类型", nameof(Order.Subtype)));
                    break;
                }
                default:
                {
                    var a = await Wfm.GetOrdersItemTopAsync(slug, null);
                    if (a?.Content?.Data != null)
                    {
                        orders.AddRange(a.Content.Data.Buy.Concat(a.Content.Data.Sell));
                    }

                    break;
                }
            }

            TopOrder = orders.Distinct().ToArray();
            _refreshKey++;
        }
        catch (Exception ex) { Log.Error(ex, "OrderTop 加载失败"); }
        finally { loading = false; }
    }

    private async Task RefreshWithRankAsync()
    {
        Log.Information("OrderTop 刷新: rank={Rank}", _selectedRank);
        if (TargetItem == null) return;
        try
        {
            var slug = TargetItem.Slug;
            var resp = await Wfm.GetOrdersItemTopAsync(slug,
                new OrderTopQueryParameter(Rank: _selectedRank, RankLt: null, Charges: null, ChargesLt: null,
                    AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null, Subtype: null));
            if (resp?.Content?.Data != null)
                TopOrder = resp.Content.Data.Buy.Concat(resp.Content.Data.Sell).Distinct().ToArray();
            _refreshKey++;
            Log.Information("OrderTop 刷新完成: rank={Rank}, count={Count}", _selectedRank, TopOrder.Length);
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

    private static ItemSubtypeSet? FallbackSubtypes(HashSet<string>? tags)
    {
        if (tags == null)
        {
            return null;
        }
        // Tags 和 Subtypes 包含相同的分类关键词
        var result = new ItemSubtypeSet();
        foreach (var t in tags)
        {
            result.Add(t);
        }

        return result;
    }
}
