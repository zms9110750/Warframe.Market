using Microsoft.AspNetCore.Components;
using WarframeMarketApp.Services;

namespace WarframeMarketApp.Pages;

public partial class Index : ComponentBase
{
	[Inject] private AppState State { get; set; } = null!;

	private string? result;
	private bool ok;
	private bool loading;
	private string stateLang => AppState.LangToStr(State.Language);
	private string statePlat => AppState.PlatToStr(State.Platform);
	private string stateCross => State.Crossplay ? "开启" : "关闭";

	private async Task TestApi()
	{
		loading = true;
		try
		{
			var response = await State.Client.GetItemsAsync();
			if (response?.Content?.Data != null)
			{
				result = $"获取到 {response.Content.Data.Length} 个可交易物品";
				ok = true;
			}
			else if (response?.Content != null && response.Content.Error != null)
			{
				result = $"API 错误: {response.Content.Error}";
				ok = false;
			}
			else
			{
				result = $"无内容 (HTTP {(int?)response?.StatusCode} {response?.StatusCode})";
				ok = false;
			}
		}
		catch (Exception ex)
		{
			result = $"异常: {ex.Message}";
			ok = false;
		}
		loading = false;
	}
}
