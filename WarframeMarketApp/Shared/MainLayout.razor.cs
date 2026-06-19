using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Serilog;
using System.Reflection;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Users;
using WarframeMarketApp.Services;

namespace WarframeMarketApp.Shared;

public partial class MainLayout : LayoutComponentBase
{
	[Inject] private AppState State { get; set; } = null!;
	[Inject] private CacheService Cache { get; set; } = null!;

	private string currentTitle = "";
	protected bool canWrite;
	protected bool clickLink;
	private List<NavItemInfo> navItems = new();

	private List<string> langItems => Enum.GetValues<Language>()
		.Where(l => l != Language.Node)
		.Select(l => AppState.LangToStr(l))
		.ToList();
	private List<string> platItems => Enum.GetValues<Platform>()
		.Select(p => AppState.PlatToStr(p))
		.ToList();

	private string selectedLang
	{
		get => AppState.LangToStr(State.Language);
		set => State.Language = AppState.StrToLang(value);
	}

	private string selectedPlatform
	{
		get => AppState.PlatToStr(State.Platform);
		set => State.Platform = AppState.StrToPlat(value);
	}

	protected override async Task OnInitializedAsync()
	{
		Log.Information("MainLayout 初始化");
		navItems = GetNavItemsFromAssembly();
		currentTitle = navItems.FirstOrDefault(s => s.Route == "/")?.Title ?? "";

		var local = await Cache.GetLocalStatusAsync();
		if (!local.HasLocalData)
		{
			Log.Information("MainLayout: 无本地数据，开始全量初始化");
			State.VersionText = "正在初始化...";
			State.IsUpdating = true;
			try
			{
				await Cache.RefreshAllAsync();
				local = await Cache.GetLocalStatusAsync();
				State.VersionText = $"数据日期 {local.UpdatedAt?[..10]}";
				Log.Information("MainLayout: 初始化完成");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "MainLayout 初始化失败");
				State.VersionText = "初始化失败";
				State.StatusMessage = ex.Message;
			}
			State.IsUpdating = false;
		}
		else
		{
			State.VersionText = $"数据日期 {local.UpdatedAt?[..10]}";
			_ = CheckVersionAsync(local.VersionId);
		}
	}

	private async Task CheckVersionAsync(string? localVersionId)
	{
		Log.Information("MainLayout: 后台版本检查");
		try
		{
			var server = await Cache.GetServerVersionAsync();
			if (server == null) { Log.Warning("MainLayout: 获取服务器版本失败"); return; }

			if (server.Id != localVersionId)
			{
				Log.Information("MainLayout: 版本不一致，后台更新 {Local} → {Server}", localVersionId, server.Id);
				State.StatusMessage = "检测到新数据，正在后台更新...";
				await Cache.RefreshAllAsync();
				State.StatusMessage = null;
				var updated = await Cache.GetLocalStatusAsync();
				State.VersionText = $"数据日期 {updated.UpdatedAt?[..10]}";
				Log.Information("MainLayout: 后台更新完成");
			}
			else Log.Information("MainLayout: 版本一致");
		}
		catch (Exception ex) { Log.Error(ex, "MainLayout 版本检查失败"); }
	}

	private async Task OnVersionClick()
	{
		Log.Information("MainLayout: 版本按钮点击");
		if (State.IsUpdating) return;

		if (State.ShowRefreshPrompt)
		{
			Log.Information("MainLayout: 执行强制刷新");
			State.IsUpdating = true;
			State.VersionText = "正在刷新...";
			State.ShowRefreshPrompt = false;
			StateHasChanged(); // 立即刷新 UI 显示 loading 状态
			try
			{
				await Cache.RefreshAllAsync();
				var local = await Cache.GetLocalStatusAsync();
				State.VersionText = $"数据日期 {local.UpdatedAt?[..10]}";
				Log.Information("MainLayout: 强制刷新完成");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "MainLayout 强制刷新失败");
				State.VersionText = "刷新失败";
			}
			State.IsUpdating = false;
		}
		else
		{
			State.VersionText = "再次点击强制刷新";
			State.ShowRefreshPrompt = true;
		}
	}

	private void OnVersionLeave()
	{
		Log.Information("MainLayout: 版本按钮失焦");
		if (State.ShowRefreshPrompt && !State.IsUpdating)
		{
			_ = RestoreVersionTextAsync();
			State.ShowRefreshPrompt = false;
		}
	}

	private async Task RestoreVersionTextAsync()
	{
		try
		{
			var local = await Cache.GetLocalStatusAsync();
			State.VersionText = local.HasLocalData ? $"数据日期 {local.UpdatedAt?[..10]}" : "无数据";
		}
		catch { State.VersionText = "无数据"; }
	}

	private List<NavItemInfo> GetNavItemsFromAssembly()
	{
		var items = new List<NavItemInfo>();
		var assembly = Assembly.GetExecutingAssembly();
		var pageTypes = assembly.GetTypes()
			.Where(t => t.GetCustomAttribute<RouteAttribute>() != null &&
					   t.GetCustomAttribute<NavItemAttribute>() != null);
		foreach (var type in pageTypes)
		{
			var routeAttr = type.GetCustomAttribute<RouteAttribute>();
			var navAttr = type.GetCustomAttribute<NavItemAttribute>();
			if (routeAttr != null && navAttr != null)
				items.Add(new NavItemInfo { Route = routeAttr.Template, Title = navAttr.Title, Icon = navAttr.Icon ?? "mdi-circle", Order = navAttr.Order });
		}
		return items.OrderBy(x => x.Order).ToList();
	}

	private class NavItemInfo
	{
		public string Route { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string Icon { get; set; } = string.Empty;
		public int Order { get; set; }
	}
}
