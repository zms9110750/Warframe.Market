using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WarframeMarketApp.Pages;

public partial class QuickReply : ComponentBase
{
	[Inject] private IJSRuntime Js { get; set; } = null!;

	[CascadingParameter(Name = "CanWrite")]
	public bool canWrite { get; set; }

	protected string newItem = "";
	protected bool copied;
	protected List<string> Tags = new();

	private const string CacheKey = "QuickReply";

	protected override async Task OnInitializedAsync()
	{
		await Task.CompletedTask;
		// TODO: 从 EF Core SQLite 加载 Tags
	}

	protected async Task Copy(string text)
	{
		try
		{
			await Js.InvokeVoidAsync("navigator.clipboard.writeText", text);
			copied = true;
		}
		catch { }
	}

	protected void Add()
	{
		if (string.IsNullOrWhiteSpace(newItem)) return;
		Tags.Add(newItem.Trim());
		newItem = "";
	}
}
