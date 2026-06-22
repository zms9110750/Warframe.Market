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
            var q = (TopOrder ?? Array.Empty<Order>()).AsEnumerable();
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

    protected List<DataTableHeader<Order>> _headers = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            if (TargetItem == null) { Log.Warning("OrderTop TargetItem 为 null"); loading = false; return; }
            Log.Information("OrderTop 初始化: {Slug}", TargetItem.Slug);
            loading = true;

            // 一次性构建完整表头（同步操作，无需等待）
            var baseHeaders = new List<DataTableHeader<Order>>
            {
                new("联系", "contact", (Func<Order, object?>)(r => (object?)null)) { Sortable = false },
                new("卖家", "seller", (Func<Order, object?>)(r => r.User?.IngameName)),
                new("声誉", "rep", (Func<Order, object?>)(r => (int?)r.User?.Reputation)),
                new("价格", "plat", (Func<Order, object?>)(r => (int?)r.Platinum)),
                new("数量", "qty", (Func<Order, object?>)(r => (int?)r.Quantity)),
            };
            var s = TargetItem.Subtypes ?? FallbackSubtypes(TargetItem.Tags);
            if (s is { IsMod: true } or { IsArcane: true })
                baseHeaders.Add(new("等级", nameof(Order.Rank)));
            else if (s is { IsAyatan: true })
            {
                baseHeaders.Add(new("琥珀星", nameof(Order.AmberStars)));
                baseHeaders.Add(new("青蓝星", nameof(Order.CyanStars)));
            }
            else if (s is { IsComponent: true } or { IsRelic: true } or { IsRiven: true })
                baseHeaders.Add(new("类型", nameof(Order.Subtype)));
            _headers = baseHeaders;

            _maxRankValue = TargetItem.MaxRank ?? 0;
            StateHasChanged();

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

    public void Dispose()
    {
        // MSlider 的 DisposeAsync 在 WebView 中会触发 JS 异常
        // https://0.0.0.1/_content/Masa.Blazor/js/masa-blazor.js
        // e.removeEventListener is not a function
        // 这个异常是不可恢复的，只能不在 Dispose 时让 MSlider 清理
    }

    private static ItemSubtypeSet? FallbackSubtypes(HashSet<string>? tags)
    {
        if (tags == null) return null;
        var result = new ItemSubtypeSet();
        foreach (var t in tags) result.Add(t);
        return result;
    }
}
