namespace WarframeMarketQueryWPF.Shared;

[AttributeUsage(AttributeTargets.Class)]
public class NavItemAttribute : Attribute
{
    public string Title { get; set; }
    public string? Icon { get; set; }
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