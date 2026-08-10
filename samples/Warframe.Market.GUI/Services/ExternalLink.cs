using System.Diagnostics;

namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>用系统默认浏览器打开外部链接。
/// PhotinoX 的 WebView 内导航会把 warframe.market 页面顶进应用窗口且无返回路径，
/// 所以所有外链点击都必须拦截并交给系统浏览器。</summary>
public static class ExternalLink
{
    public static void Open(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "打开外部链接失败: {Url}", url);
        }
    }
}
