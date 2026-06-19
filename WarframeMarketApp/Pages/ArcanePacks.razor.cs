using Microsoft.AspNetCore.Components;
using WarframeMarketApp.Data;
using WarframeMarketApp.Services;

namespace WarframeMarketApp.Pages;

public partial class ArcanePacks : ComponentBase
{
	[Inject] private ConfigService Config { get; set; } = null!;

	protected ArcanePackConfig[]? packs;

	protected override async Task OnInitializedAsync()
	{
		await Task.CompletedTask;
		packs = Config.LoadArcaneConfig();
	}
}
