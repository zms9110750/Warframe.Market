namespace zms9110750.Warframe.Market.GUI.Data;

/// <summary>UI 配置（ui-config.yaml）：快捷输入 + 钉死导航按钮</summary>
public class UIConfig
{
    public List<QuickInput> QuickInputs { get; set; } = new();
    public List<PinnedButton> PinnedButtons { get; set; } = new();
}

/// <summary>搜索框下方快捷输入模板</summary>
public class QuickInput
{
    public string Label { get; set; } = "";
    public string Query { get; set; } = "";
}

/// <summary>结果页钉死的快捷导航按钮</summary>
public class PinnedButton
{
    public string Label { get; set; } = "";
    public string Route { get; set; } = "";
    public string Icon { get; set; } = "mdi-circle";
}
