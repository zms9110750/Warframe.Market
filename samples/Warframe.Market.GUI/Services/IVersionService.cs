namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>版本信息：程序版本（配置 Version:Program）+ 数据日期（API 拉取）</summary>
public interface IVersionService
{
    /// <summary>程序版本（appsettings.json 的 Version:Program）</summary>
    string ProgramVersion { get; }

    /// <summary>拉取数据日期版本文案并写入 AppState.VersionText</summary>
    Task RefreshDataVersionAsync();
}
