using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using WarframeMarketApp.Data;

namespace WarframeMarketApp.Services;

/// <summary>
/// 持久化数据存 YAML：快捷回复 + 钉住的搜索 + 钉住的用户
/// </summary>
public class PersistentStorage
{
	private readonly string _path;
	private static readonly IDeserializer YamlDeser = new DeserializerBuilder()
		.IgnoreUnmatchedProperties()
		.WithNamingConvention(NullNamingConvention.Instance)
		.Build();
	private static readonly ISerializer YamlSer = new SerializerBuilder()
		.WithNamingConvention(NullNamingConvention.Instance)
		.Build();

	private PersistentData? _cache;

	public PersistentStorage()
	{
		_path = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"WarframeMarket", "persistent.yaml");
		Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
	}

	public PersistentData Load()
	{
		if (_cache != null) return _cache;
		if (!File.Exists(_path))
		{
			_cache = new PersistentData();
			return _cache;
		}
		try
		{
			var yaml = File.ReadAllText(_path);
			_cache = YamlDeser.Deserialize<PersistentData>(yaml) ?? new PersistentData();
		}
		catch { _cache = new PersistentData(); }
		return _cache;
	}

	public void Save()
	{
		if (_cache == null) return;
		try
		{
			var yaml = YamlSer.Serialize(_cache);
			File.WriteAllText(_path, yaml);
		}
		catch { }
	}

	public void AddQuickReply(string text)
	{
		var data = Load();
		if (!data.QuickReplies.Contains(text))
		{
			data.QuickReplies.Add(text);
			Save();
		}
	}

	public void RemoveQuickReply(string text)
	{
		var data = Load();
		data.QuickReplies.Remove(text);
		Save();
	}

	public void PinSearch(string query)
	{
		var data = Load();
		if (!data.PinnedSearches.Contains(query))
		{
			data.PinnedSearches.Add(query);
			Save();
		}
	}

	public void UnpinSearch(string query)
	{
		var data = Load();
		data.PinnedSearches.Remove(query);
		Save();
	}

	public void PinUser(string name)
	{
		var data = Load();
		if (!data.PinnedUsers.Contains(name))
		{
			data.PinnedUsers.Add(name);
			Save();
		}
	}

	public void UnpinUser(string name)
	{
		var data = Load();
		data.PinnedUsers.Remove(name);
		Save();
	}
}
