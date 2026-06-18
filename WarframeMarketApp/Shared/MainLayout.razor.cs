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
	[Inject] private zms9110750.WarframeMarketApi.WarframeMarketClient Wfm { get; set; } = null!;

	private string currentTitle = "";
	private List<NavItemInfo> navItems = new();

	// 下拉列表数据
	private List<string> langItems = new() { "en", "ko", "ru", "de", "fr", "pt", "zh-hans", "zh-hant", "es", "it", "pl", "uk", "tr", "ja" };
	private List<string> platItems = new() { "pc", "ps4", "xbox", "switch", "mobile" };

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
		navItems = GetNavItemsFromAssembly();
		currentTitle = navItems.FirstOrDefault(s => s.Route == "/")?.Title ?? "";

		State.VersionText = "正在检查版本";
		try
		{
			var v = await State.Client.GetVersionsAsync();
			if (v?.Content?.Data != null)
			{
				var date = v.Content.Data.UpdatedAtLocal;
				State.VersionText = $"数据日期 {date:yyyy-MM-dd}";
				State.VersionUpdatedAt = v.Content.Data.UpdatedAt;
				State.StatusMessage = null;
			}
			else
			{
				State.VersionText = "连接失败";
				State.StatusMessage = $"HTTP {(int?)v?.StatusCode} {v?.StatusCode}";
			}
		}
		catch (Exception ex)
		{
			State.VersionText = "连接失败";
			State.StatusMessage = ex.Message;
		}
	}

	private async Task OnVersionClick()
	{
		if (State.IsUpdating) return;

		if (State.ShowRefreshPrompt)
		{
			// 强制刷新模式：清除本地数据（这里简化，仅重新获取版本）
			State.IsUpdating = true;
			State.VersionText = "刷新中...";

			try
			{
				var v = await State.Client.GetVersionsAsync();
				if (v?.Content?.Data != null)
				{
					State.VersionText = $"数据日期 {v.Content.Data.UpdatedAtLocal:yyyy-MM-dd}";
					State.VersionUpdatedAt = v.Content.Data.UpdatedAt;
				}
				else
				{
					State.VersionText = "刷新失败";
				}
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
			State.VersionText = "再次点击，强制刷新";
			State.ShowRefreshPrompt = true;
		}
	}

	private void OnVersionLeave()
	{
		if (State.ShowRefreshPrompt && !State.IsUpdating && State.VersionUpdatedAt != null)
		{
			State.VersionText = $"数据日期 {State.VersionUpdatedAt[..10]}";
			State.ShowRefreshPrompt = false;
		}
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
			{
				items.Add(new NavItemInfo
				{
					Route = routeAttr.Template,
					Title = navAttr.Title,
					Icon = navAttr.Icon,
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
