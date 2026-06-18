using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Users;

namespace WarframeMarketApp.Services;

/// <summary>
/// 应用全局状态
/// </summary>
public class AppState
{
	private readonly WarframeMarketClient _wfm;

	public AppState(WarframeMarketClient wfm)
	{
		_wfm = wfm;
	}

	public WarframeMarketClient Client => _wfm;

	public Language Language
	{
		get => _wfm.Language;
		set => _wfm.Language = value;
	}

	public Platform Platform
	{
		get => _wfm.Platform;
		set => _wfm.Platform = value;
	}

	public bool Crossplay
	{
		get => _wfm.Crossplay;
		set => _wfm.Crossplay = value;
	}

	public string? VersionText { get; set; }
	public string? VersionUpdatedAt { get; set; }
	public bool IsUpdating { get; set; }
	public string? StatusMessage { get; set; }
}
