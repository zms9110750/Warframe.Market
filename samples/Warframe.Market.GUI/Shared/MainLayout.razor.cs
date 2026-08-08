using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Serilog;
using System.Reflection;
using zms9110750.Warframe.Market.GUI.Services;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Users;
using zms9110750.WarframeMarketApi.Services;
using ZiggyCreatures.Caching.Fusion;

namespace zms9110750.Warframe.Market.GUI.Shared;

public partial class MainLayout : LayoutComponentBase
{
    [Inject] private AppState State { get; set; } = null!;
    [Inject] private ConfigService Config { get; set; } = null!;
    [Inject] private IItemSearchService Items { get; set; } = null!;
    [Inject] private IFusionCache Cache { get; set; } = null!;

    private string currentTitle = "";
    protected bool canWrite;
    protected bool clickLink;
    private List<NavItemInfo> navItems = new();

    protected override void OnInitialized()
    {
        Log.Information("MainLayout 初始化");
        navItems = GetNavItemsFromAssembly();
        currentTitle = navItems.FirstOrDefault(s => s.Route == "/")?.Title ?? "";

        // 从 config.yaml 初始化语言/平台/跨平台
        var cfg = Config.LoadAppConfig();
        State.Language = AppState.StrToLang(cfg.DefaultLanguage);
        State.Platform = AppState.StrToPlat(cfg.DefaultPlatform);
        State.Crossplay = cfg.DefaultCrossplay;

        _ = LoadVersionAsync();
    }

    private async Task LoadVersionAsync()
    {
        try
        {
            var resp = await State.Client.GetVersionsAsync();
            var updatedAt = resp?.Content?.Data?.UpdatedAt;
            // 语义化版本无可读性，显示数据更新日期（UTC → 本地）
            if (DateTime.TryParse(updatedAt, out var dt))
            {
                State.VersionText = $"数据日期 {dt.ToLocalTime():yyyy-MM-dd}";
            }
            else
            {
                State.VersionText = $"数据日期 {updatedAt?[..Math.Min(10, updatedAt.Length)] ?? "?"}";
            }
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "获取版本失败");
            State.VersionText = "数据日期获取失败";
        }
    }

    private async Task OnVersionClick()
    {
        Log.Information("MainLayout: 版本按钮点击");
        if (State.IsUpdating)
        {
            return;
        }

        if (State.ShowRefreshPrompt)
        {
            Log.Information("MainLayout: 执行强制刷新（清 HTTP 缓存 + 重建索引）");
            State.IsUpdating = true;
            State.VersionText = "正在刷新...";
            State.ShowRefreshPrompt = false;
            StateHasChanged();
            try
            {
                // 清除全部缓存（HTTP 响应缓存 + FusionCache），重建索引
                Cache.Clear(false);
                Items.Invalidate();
                await LoadVersionAsync();
                Log.Information("MainLayout: 强制刷新完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "MainLayout 强制刷新失败");
                State.VersionText = "刷新失败";
            }
            State.IsUpdating = false;
            StateHasChanged();
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
            State.ShowRefreshPrompt = false;
            _ = LoadVersionAsync();
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
                items.Add(new NavItemInfo { Route = routeAttr.Template, Title = navAttr.Title, Icon = navAttr.Icon ?? "mdi-circle", Order = navAttr.Order });
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
