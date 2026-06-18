using System.Net.Http.Json;

namespace zms9110750.WarframeMarketApi;

/// <summary>
/// Warframe.Market API客户端
/// </summary>
public class WarframeMarketClient
{
	private readonly HttpClient _httpClient;

	public WarframeMarketClient(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}
}
