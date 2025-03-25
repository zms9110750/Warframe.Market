﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Warframe.Market.Helper;

namespace Warframe.Market.Model.LocalItems;
public record ItemCache(
		[property: JsonPropertyName("apiVersion"), JsonProperty("apiVersion")] string ApiVersion,
		[property: JsonPropertyName("data"), JsonProperty("data")] ItemShort[] Data,
		[property: JsonPropertyName("error"), JsonProperty("error")] string Error)
{
	private Dictionary<string, ItemShort> KeyOfItem
	{
		get
		{
			if (field == null)
			{
				field = [];
				foreach (var item in Data)
				{
					field.TryAdd(item.Id, item);
					field.TryAdd(item.Slug, item);
					field.TryAdd(item.I18nZH.Name, item);
					field.TryAdd(item.I18nEN.Name, item);
				}
				if (PriceCachePath != null)
				{
					foreach (var item in Data)
					{
						item.PriceCachePath = PriceCachePath;
					}
				}
			}
			return field;
		}
	} = default!;
	private TrieNode Trie
	{
		get
		{
			if (field == null)
			{
				field = new TrieNode();
				foreach (var item in KeyOfItem.Keys)
				{
					field.Add(item);
				}
			}
			return field;
		}
	} = default!;

	public IEnumerable<string> Keys => ((IReadOnlyDictionary<string, ItemShort>)KeyOfItem).Keys;

	public IEnumerable<ItemShort> Values => ((IReadOnlyDictionary<string, ItemShort>)KeyOfItem).Values;

	public int Count => KeyOfItem.Count;

	public ItemShort this[string key] => KeyOfItem.GetValueOrDefault(key)!;

	public string? PriceCachePath { get; set; }

	public IEnumerable<string> Search(string itemNamePart)
	{
		return Trie.Search(itemNamePart.ToCharArray());
	}

	public bool ContainsKey(string key)
	{
		return KeyOfItem.ContainsKey(key);
	}

	public bool TryGetValue(string key, [MaybeNullWhen(false)] out ItemShort value)
	{
		return KeyOfItem.TryGetValue(key, out value);
	}
	public IEnumerator<KeyValuePair<string, ItemShort>> GetEnumerator()
	{
		return ((IEnumerable<KeyValuePair<string, ItemShort>>)KeyOfItem).GetEnumerator();
	}
}
