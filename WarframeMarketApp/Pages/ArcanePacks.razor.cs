using Microsoft.AspNetCore.Components;
using Masa.Blazor;
using Serilog;
using WarframeMarketApp.Data;
using WarframeMarketApp.Services;

namespace WarframeMarketApp.Pages;

public partial class ArcanePacks : ComponentBase, IDisposable
{
	[Inject] private ConfigService Config { get; set; } = null!;
	[Inject] private ArcaneService Arcane { get; set; } = null!;

	private static readonly int[] PurchaseVolumes = [0, 2, 6, 15, 35];

	protected ArcanePackConfig[] Pack = [];
	protected List<DataTableHeader<ArcanePackConfig>> _headers = new();
	private CancellationTokenSource _cts = new();
	internal Dictionary<(string PackName, int Purchase), Task<double>> _tasks = new();

	protected override async Task OnInitializedAsync()
	{
		Log.Information("ArcanePacks 初始化");
		Pack = Config.LoadArcaneConfig();

		_headers.Add(new("赋能包", nameof(ArcanePackConfig.Name)));

		// 启动对所有包 × 购买量的计算
		foreach (var pack in Pack)
		{
			foreach (var count in PurchaseVolumes)
			{
				var task = Arcane.GetReferencePriceAsync(pack, count);
				_tasks[(pack.Name, count)] = task;
			}
		}

		// 动态表头（ValueExpression 只读 Name，实际值在 ItemColContent 中渲染）
		foreach (var count in PurchaseVolumes)
		{
			_headers.Add(new(count.ToString(), count.ToString())
			{
				Align = DataTableHeaderAlign.End,
				ValueExpression = pack => (object?)pack.Name
			});
		}

		// 渐进刷新
		var allTasks = Task.WhenAll(_tasks.Values);
		do
		{
			await Task.Delay(200);
			await InvokeAsync(StateHasChanged);
		} while (!allTasks.IsCompleted);

		await Task.Delay(100);
		await InvokeAsync(StateHasChanged);
	}

	public void Dispose()
	{
		_cts.Cancel();
		_cts.Dispose();
	}
}
