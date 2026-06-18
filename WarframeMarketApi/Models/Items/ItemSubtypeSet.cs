using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace zms9110750.WarframeMarketApi.Models.Items;

/// <summary>
/// 物品子类型集合。存储 API 返回的原始字符串，
/// 并提供布尔属性快速判断物品类别。
/// </summary>
[JsonConverter(typeof(ItemSubtypeSetConverter))]
public class ItemSubtypeSet : IEnumerable<string>
{
	private readonly HashSet<string> _items = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>添加子类型字符串</summary>
	public void Add(string value) => _items.Add(value);

	/// <summary>批量添加</summary>
	public void AddRange(IEnumerable<string> values)
	{
		foreach (var v in values) _items.Add(v);
	}

	/// <summary>清空</summary>
	public void Clear() => _items.Clear();

	/// <summary>是否包含指定子类型</summary>
	public bool Contains(string value) => _items.Contains(value);

	public IEnumerator<string> GetEnumerator() => _items.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

	public int Count => _items.Count;

	// ===== 快捷属性 =====

	/// <summary>是否为裂罅 MOD</summary>
	public bool IsRiven => _items.Overlaps(new[] { "rivenmod", "riven_mod", "riven" });

	/// <summary>是否为已揭示裂罅</summary>
	public bool IsRevealed => _items.Contains("revealed");

	/// <summary>是否为未揭示裂罅</summary>
	public bool IsUnrevealed => _items.Contains("unrevealed");

	/// <summary>是否为 MOD</summary>
	public bool IsMod => _items.Overlaps(new[] { "mod", "rivenmod", "riven_mod" });

	/// <summary>是否为虚空遗物</summary>
	public bool IsRelic => _items.Overlaps(new[] { "relic", "intact", "exceptional", "flawless", "radiant" });

	/// <summary>是否为鱼类</summary>
	public bool IsFish => _items.Overlaps(new[] { "fish", "small", "medium", "large" });

	/// <summary>是否为宝石</summary>
	public bool IsGem => _items.Contains("gem");

	/// <summary>是否为安魂雕塑</summary>
	public bool IsAyatan => _items.Contains("ayatan_sculpture");

	/// <summary>是否为赋能</summary>
	public bool IsArcane => _items.Overlaps(new[] { "arcane_enhancement", "arcane" });

	/// <summary>是否为 Prime 部件</summary>
	public bool IsPrimeComponent => _items.Overlaps(new[] { "prime_component", "prime" });

	/// <summary>是否为蓝图</summary>
	public bool IsBlueprint => _items.Contains("blueprint");

	/// <summary>是否为成品</summary>
	public bool IsCrafted => _items.Contains("crafted");

	/// <summary>是否为组件</summary>
	public bool IsComponent => _items.Overlaps(new[] { "component", "blueprint", "crafted" });

	/// <summary>是否为装备/武器</summary>
	public bool IsWeapon => _items.Overlaps(new[] { "weapon", "primary", "secondary", "melee", "archwing", "arch-gun", "arch-melee" });
}

internal class ItemSubtypeSetConverter : JsonConverter<ItemSubtypeSet>
{
	public override ItemSubtypeSet? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var set = new ItemSubtypeSet();
		if (reader.TokenType == JsonTokenType.StartArray)
		{
			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndArray) break;
				if (reader.TokenType == JsonTokenType.String)
					set.Add(reader.GetString()!);
			}
		}
		return set;
	}

	public override void Write(Utf8JsonWriter writer, ItemSubtypeSet value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		foreach (var item in value)
			writer.WriteStringValue(item);
		writer.WriteEndArray();
	}
}
