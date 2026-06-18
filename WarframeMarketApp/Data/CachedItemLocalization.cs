namespace WarframeMarketApp.Data;

/// <summary>
/// 物品多语言本地化（i18n FK 表）
/// </summary>
public class CachedItemLocalization
{
	public long Id { get; set; } // 自增主键
	public string ItemId { get; set; } = ""; // FK → Items.Id
	public string Language { get; set; } = ""; // "zh-hans", "en", "ko" ...
	public string Name { get; set; } = "";
	public string? Description { get; set; }
	public string? WikiLink { get; set; }
	public string Icon { get; set; } = "";
	public string Thumb { get; set; } = "";
	public string? SubIcon { get; set; }
}
