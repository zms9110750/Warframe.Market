namespace zms9110750.Warframe.Market.GUI;

/// <summary>
/// 页面导航特性：与 [Route] 一起标注在页面类上，由 MainLayout 反射自动收集进侧边栏
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class NavItemAttribute : Attribute
{
    public string Title { get; }
    public string? Icon { get; }
    public int Order { get; set; } = 1000;

    public NavItemAttribute(string title)
    {
        Title = title;
    }

    public NavItemAttribute(string title, string icon)
    {
        Title = title;
        Icon = icon;
    }
}
