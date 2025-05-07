using System.ComponentModel.DataAnnotations.Schema;

namespace Warframe.Market.Model.Items;

/// <summary>
/// 物品的多语言信息
/// </summary>
/// <param name="Name">物品的名称</param>
/// <param name="Description">物品的描述</param>
/// <param name="WikiLink">物品的Wiki链接</param>
/// <param name="Icon">物品的图标</param>
/// <param name="Thumb">物品的缩略图</param>
/// <param name="SubIcon">物品的子图标</param> 
public record Language(
	[property: JsonPropertyName("name"), JsonProperty("name")] string Name,
	[property: JsonPropertyName("icon"), JsonProperty("icon")] string Icon,
	[property: JsonPropertyName("thumb"), JsonProperty("thumb")] string Thumb,
	[property: JsonPropertyName("description"), JsonProperty("description")] string? Description = null,
	[property: JsonPropertyName("wikiLink"), JsonProperty("wikiLink")] string? WikiLink = null,
	[property: JsonPropertyName("subIcon"), JsonProperty("subIcon")] string? SubIcon = null);
