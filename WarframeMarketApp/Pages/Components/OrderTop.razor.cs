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
    protected string _userStatus = "all";
    protected int _selectedRank = 0;
    protected int _maxRankValue = 0;
    private int _refreshKey;
    private bool _showTable = true;

    protected Order[] TopOrder = Array.Empty<Order>();

    protected IEnumerable<Order> FilteredOrders
    {
        get
        {
            var q = TopOrder.AsEnumerable();
            q = q.Where(o => _showBuy ? (o.Type is "buy" or "Buy") : (o.Type is "sell" or "Sell"));
            if (_userStatus == "online")
                q = q.Where(o => o.User?.Status is "online" or "ingame");
            else if (_userStatus == "ingame")
                q = q.Where(o => o.User?.Status == "ingame");
            if (_maxRankValue > 0 && _selectedRank > 0)
            {
                if (_showBuy)
                    q = q.Where(o => (o.Rank ?? 0) >= _selectedRank);
                else
                    q = q.Where(o => (o.Rank ?? 0) <= _selectedRank);
            }
            return q;
        }
    }

    protected List<DataTableHeader<Order>> _headers = new()
    {
        new("联系", nameof(Order.Id)) { Sortable = false },
        new("卖家", nameof(Order.User), (Func<Order, object?>)(r => r.User?.IngameName)),
        new("声誉", nameof(Order.User), (Func<Order, object?>)(r => r.User?.Reputation)),
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
            StateHasChanged();

            var s = TargetItem.Subtypes ?? FallbackSubtypes(TargetItem.Tags);
            if (s is { IsMod: true } or { IsArcane: true })
                _headers.Add(new("等级", nameof(Order.Rank)));
            else if (s is { IsAyatan: true })
            {
                _headers.Add(new("琥珀星", nameof(Order.AmberStars)));
                _headers.Add(new("青蓝星", nameof(Order.CyanStars)));
            }
            else if (s is { IsComponent: true } or { IsRelic: true } or { IsRiven: true })
                _headers.Add(new("类型", nameof(Order.Subtype)));

            var slug = TargetItem.Slug;
            var resp = await Wfm.GetOrdersItemAsync(slug);
            if (resp?.Content?.Data != null)
                TopOrder = resp.Content.Data;
            _refreshKey++;
            Log.Information("OrderTop 初始化完成: {Slug}, count={Count}", slug, TopOrder.Length);
        }
        catch (Exception ex) { Log.Error(ex, "OrderTop 加载失败"); }
        finally { loading = false; }
    }

    protected async Task CopyAsync(string text)
    {
        try { await Js.InvokeVoidAsync("navigator.clipboard.writeText", text); }
        catch { }
    }

    public void Dispose() { }

    private static ItemSubtypeSet? FallbackSubtypes(HashSet<string>? tags)
    {
        if (tags == null) return null;
        var result = new ItemSubtypeSet();
        foreach (var t in tags) result.Add(t);
        return result;
    }
}
