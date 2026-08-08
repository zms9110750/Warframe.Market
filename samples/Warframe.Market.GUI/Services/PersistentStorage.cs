using System.IO;
using zms9110750.Warframe.Market.GUI.Data;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>
/// 用户数据持久化：快捷回复 + 钉住的搜索 + 钉住的用户，存 %LocalAppData%\WarframeMarket\persistent.yaml
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
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WarframeMarket");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "persistent.yaml");
    }

    public PersistentData Load()
    {
        if (_cache != null)
        {
            return _cache;
        }

        if (!File.Exists(_path))
        {
            _cache = new PersistentData();
            return _cache;
        }
        try
        {
            _cache = YamlDeser.Deserialize<PersistentData>(File.ReadAllText(_path)) ?? new PersistentData();
        }
        catch { _cache = new PersistentData(); }
        return _cache;
    }

    public void Save()
    {
        if (_cache == null)
        {
            return;
        }

        try { File.WriteAllText(_path, YamlSer.Serialize(_cache)); }
        catch { }
    }

    public void AddQuickReply(string text)
    {
        AddTo(Load().QuickReplies, text);
    }

    public void RemoveQuickReply(string text)
    {
        RemoveFrom(Load().QuickReplies, text);
    }

    public void PinSearch(string query)
    {
        AddTo(Load().PinnedSearches, query);
    }

    public void UnpinSearch(string query)
    {
        RemoveFrom(Load().PinnedSearches, query);
    }

    public void PinUser(string name)
    {
        AddTo(Load().PinnedUsers, name);
    }

    public void UnpinUser(string name)
    {
        RemoveFrom(Load().PinnedUsers, name);
    }

    private void AddTo(List<string> list, string item)
    {
        if (!list.Contains(item)) { list.Add(item); Save(); }
    }

    private void RemoveFrom(List<string> list, string item)
    {
        if (list.Remove(item))
        {
            Save();
        }
    }
}
