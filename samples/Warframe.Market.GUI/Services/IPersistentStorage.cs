using zms9110750.Warframe.Market.GUI.Data;

namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>用户数据持久化（快捷回复/钉住搜索/钉住用户，存 persistent.yaml）</summary>
public interface IPersistentStorage
{
    /// <summary>读持久化数据（首次读盘，后续缓存）</summary>
    PersistentData Load();

    void AddQuickReply(string text);
    void RemoveQuickReply(string text);
    void PinSearch(string query);
    void UnpinSearch(string query);
    void PinUser(string name);
    void UnpinUser(string name);
}
