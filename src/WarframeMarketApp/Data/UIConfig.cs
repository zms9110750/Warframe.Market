namespace WarframeMarketApp.Data;

/// <summary>
/// UI 配置（从 YAML/TOML 加载）：快捷输入、钉死按钮
/// </summary>
public class UIConfig
{
	/// <summary>快捷输入模板，显示在搜索框下方</summary>
	public QuickInputConfig[] QuickInputs { get; set; } = [];

	/// <summary>查询结果页面上的钉死快捷按钮</summary>
	public PinnedButtonConfig[] PinnedButtons { get; set; } = [];
}

public class QuickInputConfig
{
	public string Label { get; set; } = "";
	public string Query { get; set; } = "";
	public string? Icon { get; set; }
}

public class PinnedButtonConfig
{
	public string Label { get; set; } = "";
	public string Route { get; set; } = "";
	public string Icon { get; set; } = "mdi-circle";
}
