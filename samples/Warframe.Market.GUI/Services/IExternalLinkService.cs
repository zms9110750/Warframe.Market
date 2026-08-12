namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>用系统默认浏览器打开外部链接（PhotinoX WebView 内导航会顶走应用窗口，必须拦截）</summary>
public interface IExternalLinkService
{
    void Open(string url);
}
