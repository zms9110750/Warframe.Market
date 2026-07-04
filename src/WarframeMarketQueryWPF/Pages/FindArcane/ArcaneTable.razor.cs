using Masa.Blazor;
using Microsoft.Extensions.Caching.Memory;
using WarframeMarketQuery.Extension;

namespace WarframeMarketQueryWPF.Pages.FindArcane;

public partial class ArcaneTable : IDisposable
{
    private List<DataTableHeader<string>> _headersItem = [];
    Dictionary<string, string> NamePairs = [];
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _headersItem.Add(new("名称", "名称") { ValueExpression = t => t });
        _headersItem.Add(new("出货率%", "出货率")
        {
            Align = DataTableHeaderAlign.End,
            ValueExpression = name => Pack.GetProbability(name) * 100
        });
        var option = new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(1) };
        _headersItem.Add(new("参考价格", "参考价格")
        {
            Align = DataTableHeaderAlign.End,
            ValueExpression = name => WfmApi.GetStatisticByIndexAsync(name) is { IsCompleted: true } task ? task.Result.GetMaterialBasedReferencePrice() : null
        });
        _headersItem.Add(new("出货率x价格", "出货率x价格")
        {
            Align = DataTableHeaderAlign.End,
            ValueExpression = name => WfmApi.GetStatisticByIndexAsync(name) is { IsCompleted: true } task ? task.Result.GetMaterialBasedReferencePrice() * Pack.GetProbability(name) : null
        });
        _headersItem.Add(new("日均交易量", "日均交易量")
        {
            Align = DataTableHeaderAlign.End,
            ValueExpression = name => WfmApi.GetStatisticByIndexAsync(name) is { IsCompleted: true } task ? task.Result.Payload.StatisticsClosed.Day90.Sum(entry => entry.Volume * SyntheticConsumption[entry.ModRank ?? 0]) / 90.0 / SyntheticConsumption[task.Result.Payload.StatisticsClosed.Day90.Max(s => s.ModRank) ?? 0] : null
        });
        HashSet<Task> geting = [];
        foreach (var item in Pack.SelectMany(s => s))
        {
            var itemshort = await WfmApi.GetItemByIndexAsync(item);
            NamePairs[item] = itemshort.Slug;
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
    static IReadOnlyList<int> SyntheticConsumption => ModelExtension.SyntheticConsumption;
    CancellationTokenSource CancellationTokenSource = new();

    public void Dispose()
    {
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
    }
}
