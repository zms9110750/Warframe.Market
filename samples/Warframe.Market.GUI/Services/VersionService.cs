using Microsoft.Extensions.Configuration;
using Serilog;
using zms9110750.WarframeMarketApi;

namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>版本信息实现：程序版本从配置读；数据日期从 API 拉取</summary>
public class VersionService : IVersionService
{
    private readonly IConfiguration _config;
    private readonly WarframeMarketClient _wfm;
    private readonly IAppStateService _state;

    public VersionService(IConfiguration config, WarframeMarketClient wfm, IAppStateService state)
    {
        _config = config;
        _wfm = wfm;
        _state = state;
    }

    public string ProgramVersion => _config["Version:Program"] ?? "?";

    public async Task RefreshDataVersionAsync()
    {
        try
        {
            var resp = await _wfm.GetVersionsAsync();
            var updatedAt = resp?.Content?.Data?.UpdatedAt;
            // 语义化版本无可读性，显示数据更新日期（UTC → 本地）
            if (DateTime.TryParse(updatedAt, out var dt))
            {
                _state.VersionText = $"数据日期 {dt.ToLocalTime():yyyy-MM-dd}";
            }
            else
            {
                _state.VersionText = $"数据日期 {updatedAt?[..Math.Min(10, updatedAt.Length)] ?? "?"}";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "获取版本失败");
            _state.VersionText = "数据日期获取失败";
        }
    }
}
