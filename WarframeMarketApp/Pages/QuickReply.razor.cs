using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Serilog;
using WarframeMarketApp.Data;

namespace WarframeMarketApp.Pages;

public partial class QuickReply : ComponentBase
{
	[Inject] private IJSRuntime Js { get; set; } = null!;
	[Inject] private IServiceScopeFactory ScopeFactory { get; set; } = null!;

	[CascadingParameter(Name = "CanWrite")]
	public bool canWrite { get; set; }

	protected string newItem = "";
	protected string? copied;
	protected List<QuickReplyItem> Tags = new();

	protected override async Task OnInitializedAsync()
	{
		Log.Information("QuickReply 初始化");
		using var scope = ScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<WfmDbContext>();
		Tags = await db.QuickReplies.OrderBy(q => q.SortOrder).ToListAsync();
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
		await Add();
	}

	protected async Task Add()
	{
		if (string.IsNullOrWhiteSpace(newItem)) return;

		using var scope = ScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<WfmDbContext>();
		var maxOrder = Tags.Count > 0 ? Tags.Max(t => t.SortOrder) : 0;
		var item = new QuickReplyItem { Text = newItem.Trim(), SortOrder = maxOrder + 1 };
		db.QuickReplies.Add(item);
		await db.SaveChangesAsync();
		Tags.Add(item);
		newItem = "";
	}

	protected async Task Remove(QuickReplyItem item)
	{
		using var scope = ScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<WfmDbContext>();
		db.QuickReplies.Remove(item);
		await db.SaveChangesAsync();
		Tags.Remove(item);
	}

	protected async Task SaveOnBlur(QuickReplyItem item)
	{
		if (string.IsNullOrWhiteSpace(item.Text))
		{
			await Remove(item);
			return;
		}

		using var scope = ScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<WfmDbContext>();
		db.QuickReplies.Update(item);
		await db.SaveChangesAsync();
	}
}
