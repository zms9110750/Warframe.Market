using Masa.Blazor;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using WarframeMarketQuery.Extension;
using WarframeMarketQuery.Model.Items;

namespace WarframeMarketQueryWPF.Pages.FindItem;

public partial class QueryTrie : IDisposable
{
    List<DataTableHeader<ItemShort>> _headers = [];
    CancellationTokenSource CancellationTokenSource = new();
    List<ItemShort> Items { get; set; } = new List<ItemShort>();

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _headers.Add(new("中文名称", "ZhCh")
        {
            ValueExpression = r => r.I18n[Language.ZhHans].Name
        });

        _headers.Add(new("英文名称", "En")
        {
            ValueExpression = r => r.I18n[Language.En].Name
        });
        _headers.Add(new("价格", "Price")
        {
            Align = DataTableHeaderAlign.End,
            ValueExpression = r => WfmApi.GetStatisticAsync(r, CancellationTokenSource.Token) is { IsCompleted: true } statistic ? statistic.Result.GetReferencePrice() : null
        });
        _headers.Add(new("满级价格", "MaxPrice")
        {
            Align = DataTableHeaderAlign.End,
            ValueExpression = r => WfmApi.GetStatisticAsync(r, CancellationTokenSource.Token) is { IsCompleted: true } statistic ? statistic.Result.GetMaxReferencePrice() : null
        });
        _headers.Add(new() { Text = "在线订单", Value = "data-table-expand" });
        disposables.Add(CancellationTokenSource);
    }
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        HashSet<Task> geting = [];
        var results = await Task.Run(() => trie.Search(Title));
        foreach (var item in results)
        {
            var itemshort = await WfmApi.GetItemByIndexAsync(item);
            Items.Add(itemshort);
            var stat = WfmApi.GetStatisticAsync(itemshort, CancellationTokenSource.Token);
            if (!stat.IsCompleted)
            {
                geting.Add(stat.AsTask());
            }
        }
        var allTasks = Task.WhenAll(geting);
        do
        {
            await Task.Delay(200);
            await InvokeAsync(StateHasChanged);
        } while (!allTasks.IsCompleted);
    }
    private CompositeDisposable disposables = new CompositeDisposable();
    public void Dispose()
    {
        CancellationTokenSource.Cancel();
        disposables?.Dispose();
    }
}

