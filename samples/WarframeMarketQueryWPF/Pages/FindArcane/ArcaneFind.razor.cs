using Masa.Blazor;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Reactive.Disposables;
using WarframeMarketQuery.Arcane;
using WarframeMarketQuery.Extension;
using ZiggyCreatures.Caching.Fusion;
namespace WarframeMarketQueryWPF.Pages.FindArcane;

public partial class ArcaneFind : IDisposable
{
    private List<DataTableHeader<ArcanePack>> _headers =
      [
             new ("赋能包",nameof(ArcanePack.Name))
      ];
    ArcanePack[] Pack = [];
    CompositeDisposable _disposables = new();
    CancellationTokenSource TokenSource = new CancellationTokenSource();
    Dictionary<(ArcanePack, int), Task<double>> ReferencePrice { get; } = new Dictionary<(ArcanePack, int), Task<double>>();
    int i = 0;
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Pack = await Fusion.GetOrSetAsync(GetType().Name
        , c => Task.Run(() => new ConfigurationBuilder().AddYamlFile("赋能包配置.yaml").Build().GetSection("赋能包配置").Get<ArcanePack[]>()!)
        , op => op.SetSkipDistributedCache(true, null));

        HashSet<Task> geting = [];

        foreach (var item in Pack)
        {
            foreach (var count in (int[])[0, 2, 6, 15, 35])
            {
                var task = item.GetReferencePriceAsync(WfmApi, count, TokenSource.Token);
                geting.Add(task);
                ReferencePrice[(item, count)] = task;
            }
        }
        foreach (var count in (int[])[0, 2, 6, 15, 35])
        {
            _headers.Add(new(count.ToString(), count.ToString())
            {
                Align = DataTableHeaderAlign.End,
                ValueExpression = pack =>
                ReferencePrice.TryGetValue((pack, count), out var task) && task.IsCompleted
                ? task.Result
                : null
            });
        }
        var allTasks = Task.WhenAll(geting);
        do
        {
            await Task.Delay(200);
            await InvokeAsync(StateHasChanged);
        } while (!allTasks.IsCompleted);
        await Task.Delay(200);
        await InvokeAsync(StateHasChanged);
    }
    public void Dispose()
    {
        TokenSource.Cancel();
        TokenSource.Dispose();
        _disposables.Dispose();
    }
}
