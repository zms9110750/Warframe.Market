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
    private bool _showTable = true;

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

            // 立即显示滑块（MaxRank 从本地数据获取，不需要 API）
            _maxRankValue = TargetItem.MaxRank ?? 0;
            StateHasChanged();

            // 根据物品类型动态加列
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

            // 一次 API 请求获取全部等级数据
            var slug = TargetItem.Slug;
            var resp = await Wfm.GetOrdersItemTopAsync(slug,
                _maxRankValue > 0
                    ? new OrderTopQueryParameter(Rank: null, RankLt: _maxRankValue, Charges: null, ChargesLt: null,
                        AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null, Subtype: null)
                    : null);
            if (resp?.Content?.Data != null)
                TopOrder = resp.Content.Data.Buy.Concat(resp.Content.Data.Sell).Distinct().ToArray();
            _refreshKey++;
            Log.Information("OrderTop 初始化完成: {Slug}, count={Count}", slug, TopOrder.Length);
        }
        catch (Exception ex) { Log.Error(ex, "OrderTop 加载失败"); }
        finally { loading = false; }
    }

    private async Task RefreshWithRankAsync()
    {
        Log.Information("OrderTop 刷新: rankLt={Rank}, showBuy={Buy}", _selectedRank, _showBuy);
        if (TargetItem == null) return;
        try
        {
            var slug = TargetItem.Slug;
            // 购→RankLt(范围广)，售→Rank(精确等级)
            OrderTopQueryParameter? query = null;
            if (_selectedRank > 0)
            {
                if (_showBuy)
                    query = new(Rank: null, RankLt: _selectedRank, Charges: null, ChargesLt: null,
                        AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null, Subtype: null);
                else
                    query = new(Rank: _selectedRank, RankLt: null, Charges: null, ChargesLt: null,
                        AmberStars: null, AmberStarsLt: null, CyanStars: null, CyanStarsLt: null, Subtype: null);
            }
            var resp = await Wfm.GetOrdersItemTopAsync(slug, query);
            if (resp?.Content?.Data != null)
            {
                _showTable = false;
                StateHasChanged();
                TopOrder = resp.Content.Data.Buy.Concat(resp.Content.Data.Sell).Distinct().ToArray();
                _showTable = true;
            }
            _refreshKey++;
            Log.Information("OrderTop 刷新完成: rankLt={Rank}, count={Count}", _selectedRank, TopOrder.Length);
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
        if (tags == null) return null;
        var result = new ItemSubtypeSet();
        foreach (var t in tags) result.Add(t);
        return result;
    }
}
