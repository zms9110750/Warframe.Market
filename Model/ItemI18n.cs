using Newtonsoft.Json;
using System.Text.Json.Serialization;
namespace Warframe.Market.Model;


/// <summary>
/// 表示物品的多语言信息。
/// </summary>
/// <param name="Name"> 物品的名称。 </param>
/// <param name="Description"> 物品的描述。 </param>
/// <param name="WikiLink"> 物品的 Wiki 链接。 </param>
/// <param name="Icon"> 物品的图标 URL。 </param>
/// <param name="Thumb"> 物品的缩略图 URL。 </param>
/// <param name="SubIcon"> 物品的子图标 URL。 </param>
public record ItemI18n([property: JsonPropertyName("name"), JsonProperty("name")] string Name,
 [property: JsonPropertyName("description"), JsonProperty("description")] string Description,
 [property: JsonPropertyName("wikiLink"), JsonProperty("wikiLink")] string WikiLink,
 [property: JsonPropertyName("icon"), JsonProperty("icon")] string Icon,
 [property: JsonPropertyName("thumb"), JsonProperty("thumb")] string Thumb,
 [property: JsonPropertyName("subIcon"), JsonProperty("subIcon")] string SubIcon);
