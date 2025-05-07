using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Warframe.Market.Model.Items;

/// <summary>
/// 物品的多语言包（遵循 .NET CultureInfo 地区码规范）
/// </summary>
/// <param name="Ko">韩语（韩国）语言包</param>
/// <param name="Ru">俄语（俄罗斯）语言包</param>
/// <param name="De">德语（德国）语言包</param>
/// <param name="Fr">法语（法国）语言包</param>
/// <param name="Pt">葡萄牙语（巴西）语言包</param>
/// <param name="ZhHans">简体中文（中国）语言包</param>
/// <param name="ZhHant">繁体中文（中国台湾）语言包</param>
/// <param name="Es">西班牙语（西班牙）语言包</param>
/// <param name="It">意大利语（意大利）语言包</param>
/// <param name="Pl">波兰语（波兰）语言包</param>
/// <param name="Uk">乌克兰语（乌克兰）语言包</param>
/// <param name="En">英语（美国）语言包</param>
[JsonObject]
[method: System.Text.Json.Serialization.JsonConstructor]
[method: Newtonsoft.Json.JsonConstructor]
public record ItemI18n(
	[property: JsonPropertyName("en"), JsonProperty("en")] Language En,
	[property: JsonPropertyName("ko"), JsonProperty("ko")] Language? Ko = null,
	[property: JsonPropertyName("ru"), JsonProperty("ru")] Language? Ru = null,
	[property: JsonPropertyName("de"), JsonProperty("de")] Language? De = null,
	[property: JsonPropertyName("fr"), JsonProperty("fr")] Language? Fr = null,
	[property: JsonPropertyName("pt"), JsonProperty("pt")] Language? Pt = null,
	[property: JsonPropertyName("es"), JsonProperty("es")] Language? Es = null,
	[property: JsonPropertyName("it"), JsonProperty("it")] Language? It = null,
	[property: JsonPropertyName("pl"), JsonProperty("pl")] Language? Pl = null,
	[property: JsonPropertyName("uk"), JsonProperty("uk")] Language? Uk = null,
	[property: JsonPropertyName("zh-hans"), JsonProperty("zh-hans")] Language? ZhHans = null,
	[property: JsonPropertyName("zh-hant"), JsonProperty("zh-hant")] Language? ZhHant = null
	) : IEnumerable<KeyValuePair<string, Language>>, ITuple
{
	public ItemI18n(IReadOnlyDictionary<string, Language> pairs)
	   : this(pairs[LanguageCode[0]],
			 pairs.GetValueOrDefault(LanguageCode[1]),
			 pairs.GetValueOrDefault(LanguageCode[2]),
			 pairs.GetValueOrDefault(LanguageCode[3]),
			 pairs.GetValueOrDefault(LanguageCode[4]),
			 pairs.GetValueOrDefault(LanguageCode[5]),
			 pairs.GetValueOrDefault(LanguageCode[6]),
			 pairs.GetValueOrDefault(LanguageCode[7]),
			 pairs.GetValueOrDefault(LanguageCode[8]),
			 pairs.GetValueOrDefault(LanguageCode[9]),
			 pairs.GetValueOrDefault(LanguageCode[10]),
			 pairs.GetValueOrDefault(LanguageCode[11]))
	{ }

	int ITuple.Length => 12;

	object? ITuple.this[int index] => index switch
	{
		0 => En,
		1 => Ko,
		2 => Ru,
		3 => De,
		4 => Fr,
		5 => Pt,
		6 => Es,
		7 => It,
		8 => Pl,
		9 => Uk,
		10 => ZhHans,
		11 => ZhHant,
		_ => new ArgumentOutOfRangeException(nameof(index), "must in [0 .. 11]")
	};
	static string[] LanguageCode { get; } = ["En", "Ko", "Ru", "De", "Fr", "Pt", "Es", "It", "Pl", "Uk", "ZhHans", "ZhHant",];
	public IEnumerator<KeyValuePair<string, Language>> GetEnumerator()
	{
		ITuple tuple = this;
		for (int i = 0; i < tuple.Length; i++)
		{
			if (tuple[i] is Language language)
			{
				yield return new KeyValuePair<string, Language>(LanguageCode[i], language);
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}