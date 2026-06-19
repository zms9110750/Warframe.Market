using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
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
	/// <summary>是否显示为链接模式</summary>
	protected bool clickLink;
	private List<NavItemInfo> navItems = new();

	// ─── 语言/平台下拉 ───

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

	// ─── 初始化 ───

	protected override async Task OnInitializedAsync()
	{
		navItems = GetNavItemsFromAssembly();
		currentTitle = navItems.FirstOrDefault(s => s.Route == "/")?.Title ?? "";

		// 检查本地数据状态
		var local = await Cache.GetLocalStatusAsync();
		if (!local.HasLocalData)
		{
			// 无数据：触发全量拉取
			State.VersionText = "正在初始化...";
			State.IsUpdating = true;
			try
			{
				await Cache.RefreshAllAsync();
				local = await Cache.GetLocalStatusAsync();
				State.VersionText = $"数据日期 {local.UpdatedAt?[..10]}";
			}
			catch (Exception ex)
			{
				State.VersionText = "初始化失败";
				State.StatusMessage = ex.Message;
			}
			State.IsUpdating = false;
		}
		else
		{
			// 有本地数据：显示日期，后台查版本
			State.VersionText = $"数据日期 {local.UpdatedAt?[..10]}";
			_ = CheckVersionAsync(local.VersionId);
		}
	}

	// ─── 后台版本检查 ───

	private async Task CheckVersionAsync(string? localVersionId)
	{
		try
		{
			var server = await Cache.GetServerVersionAsync();
			if (server == null) return;

			if (server.Id != localVersionId)
			{
				// 版本不一致：后台更新
				State.StatusMessage = "检测到新数据，正在后台更新...";
				await Cache.RefreshAllAsync();
				State.StatusMessage = null;
				var updated = await Cache.GetLocalStatusAsync();
				State.VersionText = $"数据日期 {updated.UpdatedAt?[..10]}";
			}
		}
		catch { /* 静默失败，下次再看 */ }
	}

	// ─── 版本按钮 4 态 ───

	private async Task OnVersionClick()
	{
		if (State.IsUpdating) return;

		if (State.ShowRefreshPrompt)
		{
			// 状态 4：再次点击 → 强制刷新
			State.IsUpdating = true;
			State.VersionText = "正在刷新...";
			try
			{
				await Cache.RefreshAllAsync();
				var local = await Cache.GetLocalStatusAsync();
				State.VersionText = $"数据日期 {local.UpdatedAt?[..10]}";
			}
			catch
			{
				State.VersionText = "刷新失败";
			}
			State.IsUpdating = false;
			State.ShowRefreshPrompt = false;
		}
		else
		{
			// 状态 3：点击 → 提示强制刷新
			State.VersionText = "再次点击强制刷新";
			State.ShowRefreshPrompt = true;
		}
	}

	private void OnVersionLeave()
	{
		if (State.ShowRefreshPrompt && !State.IsUpdating)
		{
			// 失焦还原
			_ = RestoreVersionTextAsync();
			State.ShowRefreshPrompt = false;
		}
	}

	private async Task RestoreVersionTextAsync()
	{
		try
		{
			var local = await Cache.GetLocalStatusAsync();
			if (local.HasLocalData)
				State.VersionText = $"数据日期 {local.UpdatedAt?[..10]}";
			else
				State.VersionText = "无数据";
		}
		catch
		{
			State.VersionText = "无数据";
		}
	}

	// ─── 导航 ───

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
			{
				items.Add(new NavItemInfo
				{
					Route = routeAttr.Template,
					Title = navAttr.Title,
					Icon = navAttr.Icon ?? "mdi-circle",
					Order = navAttr.Order
				});
			}
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
