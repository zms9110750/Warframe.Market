using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WarframeMarketApp.Pages;

public partial class QuickReply : ComponentBase
{
	[Inject] private IJSRuntime Js { get; set; } = null!;

	protected string? copied;

	protected readonly (string, string)[] templates = new[]
	{
		("询价", "/w {user} Hi! I want to buy your {item} for {plat} platinum. (Warframe Market)"),
		("卖单", "/w {user} Hi! I want to sell my {item} for {plat} platinum. (Warframe Market)"),
		("求购", "/w {user} Hi! I want to buy your {item} platinum. (Warframe Market)"),
		("交易完成", "/w {user} Thank you for the trade! Good luck!"),
		("邀请", "/w {user} Hi! Do you still have {item} for {plat} platinum?"),
	};

	protected async Task Copy(string text)
	{
		try
		{
			await Js.InvokeVoidAsync("navigator.clipboard.writeText", text);
			copied = text.Length > 40 ? text[..40] + "..." : text;
		}
		catch { }
	}
}
