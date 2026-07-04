using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Serilog;
using WarframeMarketApp.Services;

namespace WarframeMarketApp.Pages;

public partial class QuickReply : ComponentBase
{
	[Inject] private IJSRuntime Js { get; set; } = null!;
	[Inject] private PersistentStorage Storage { get; set; } = null!;

	[CascadingParameter(Name = "CanWrite")]
	public bool canWrite { get; set; }

	protected string newItem = "";
	private int _inputKey;
	protected string? copied;
	protected List<string> Tags = new();

	protected override void OnInitialized()
	{
		Log.Information("QuickReply 初始化");
		Tags = Storage.Load().QuickReplies;
	}

	protected async Task Copy(string text)
	{
		Log.Information("QuickReply 复制: {Text}", text.Length > 30 ? text[..30] + "..." : text);
		try
		{
			await Js.InvokeVoidAsync("navigator.clipboard.writeText", text);
			copied = text.Length > 40 ? text[..40] + "..." : text;
		}
		catch { }
	}

	protected async Task AddNew()
	{
		if (string.IsNullOrWhiteSpace(newItem)) return;
		Tags.Add(newItem.Trim());
		Storage.AddQuickReply(newItem.Trim());
		newItem = "";
		_inputKey++;
		await InvokeAsync(StateHasChanged);
	}

	protected void Remove(string text)
	{
		Tags.Remove(text);
		Storage.RemoveQuickReply(text);
	}

	protected void SaveOnBlur(string oldText, string newText)
	{
		if (string.IsNullOrWhiteSpace(newText))
		{
			Tags.Remove(oldText);
			Storage.RemoveQuickReply(oldText);
			return;
		}
		if (oldText != newText)
		{
			var idx = Tags.IndexOf(oldText);
			if (idx >= 0) Tags[idx] = newText;
			Storage.RemoveQuickReply(oldText);
			Storage.AddQuickReply(newText);
		}
	}
}
