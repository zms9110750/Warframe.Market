using Masa.Blazor;
using System.Reactive.Disposables;
using WarframeMarketQuery.Extension;
using WarframeMarketQuery.Model.Items;
using WarframeMarketQuery.Model.Orders;
using WarframeMarketQuery.Model.Statistics;
namespace WarframeMarketQueryWPF.Pages.FindUser;

public partial class QueryUser : IDisposable
{
    bool isLoading = true;
    private List<DataTableHeader<Model>> _headers =
  [
        new("中文名称", nameof(Model.ZhCn)) ,
        new("英文名称", nameof(Model.En))   ,
        new("订单价格", nameof(Model.Price))
        {
         Align = DataTableHeaderAlign.End
        },
        new("参考价格", nameof(Model.RefPrice))
        {
         Align = DataTableHeaderAlign.End
        },
        new ("差价", nameof(Model.Difference))
        {
         Align = DataTableHeaderAlign.End
        },  new ("买/卖", nameof(Model.Type))
        {
         Align = DataTableHeaderAlign.End
        },
         new (){ Text="在线订单",Value="data-table-expand"}
  ];
    class Model(Order order, ItemShort item, Task<Statistic> statistic)
    {
        public ItemShort Item { get; } = item;
        public Order Order { get; } = order;
        public string ZhCn => Item.I18n[Language.ZhHans].Name;
        public string En => Item.I18n[Language.En].Name;
        public double Price => Order.Platinum;
        public double? RefPrice => statistic.IsCompleted ? statistic.Result.GetReferencePrice(en => en.ModRank == Order.Rank && en.Subtype == Order.Subtype && en.AmberStars == Order.AmberStars) : null;
        public double? Difference => RefPrice - Price;
        public OrderType Type => Order.Type;
    }
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        disposables.Add(CancellationTokenSource);
        var orders = await WfmApi.GetOrdersFromUserAsync(UserName);
        isLoading = false;
        if (orders == null)
        {
            NotFound = true;
            return;
        }
        HashSet<Task> geting = [];
        foreach (var order in orders)
        {
            var item = await WfmApi.GetItemByIndexAsync(order);
            var statistic = WfmApi.GetStatisticAsync(item, CancellationTokenSource.Token).AsTask();
            models.Add(new Model(order, item, statistic));
            geting.Add(statistic);
        }
        var allTasks = Task.WhenAll(geting);
        do
        {
            await Task.Delay(200);
            await InvokeAsync(StateHasChanged);
        } while (!allTasks.IsCompleted);

    }
    bool NotFound;
    CancellationTokenSource CancellationTokenSource = new();
    List<Model> models = [];
    private CompositeDisposable disposables = new CompositeDisposable();
    public void Dispose()
    {
        CancellationTokenSource.Cancel();
        disposables?.Dispose();
    }
}
