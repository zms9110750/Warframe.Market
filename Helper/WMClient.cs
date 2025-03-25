using Polly.Retry;
using Polly;
using System.Net;
using Warframe.Market.Model.Statistics;
using System.Net.Http.Json;
using Warframe.Market.Model.Items;
using Warframe.Market.Model.ItemsSet;
using Warframe.Market.Model.LocalItems;

namespace Warframe.Market.Helper;

public class WMClient
{
	HttpClient Http { get; }
	ThrottleGate Throttle { get; }
	AsyncRetryPolicy Http429RetryPolicy { get; }
	public WMClient(TimeSpan? interval = null, HttpClient? http = null, AsyncRetryPolicy? retryPolicy = null)
	{
		Http = http ??= new HttpClient();
		http.DefaultRequestHeaders.Add("Language", "zh-hans");
		Throttle = new ThrottleGate(interval ?? TimeSpan.FromSeconds(1.0 / 3));
		Http429RetryPolicy = retryPolicy ?? Policy
			.Handle<HttpRequestException>(ex => ex.StatusCode == HttpStatusCode.TooManyRequests)
			.WaitAndRetryAsync(retryCount: 5, sleepDurationProvider: (retryCount) => TimeSpan.FromMilliseconds(Random.Shared.Next(500) + 500 * retryCount));
	}
	public async Task<string> GetStringAsync(string url)
	{
		await Throttle;
		return await Http429RetryPolicy.ExecuteAsync(() => Http.GetStringAsync(url));
	}
	public async Task<T> GetFromJsonAsync<T>(string url)
	{
		await Throttle;
		return (await Http429RetryPolicy.ExecuteAsync(() => Http.GetFromJsonAsync<T>(url)))!;
	}
	public Task<Model.Versions.Version> GetVersionAsync()
	{
		return GetFromJsonAsync<Model.Versions.Version>($"https://api.warframe.market/v2/versions");
	}
	public Task<Item> GetItemAsync(string slug)
	{
		return GetFromJsonAsync<Item>($"https://api.warframe.market/v2/item/{slug}");
	}
	public Task<Item> GetItemAsync(ItemShort item)
	{
		return GetFromJsonAsync<Item>($"https://api.warframe.market/v2/item/{item.Slug}");
	}
	public Task<ItemSet> GetItemSetAsync(string slug)
	{
		return GetFromJsonAsync<ItemSet>($"https://api.warframe.market/v2/item/{slug}/set");
	}
	public Task<ItemSet> GetItemSetAsync(ItemShort item)
	{
		return GetFromJsonAsync<ItemSet>($"https://api.warframe.market/v2/item/{item.Slug}/set");
	}
	public Task<Statistic> GetStatisticsAsync(string slug)
	{
		return GetFromJsonAsync<Statistic>($"https://api.warframe.market/v1/items/{slug}/statistics");
	}
	public Task<Statistic> GetStatisticsAsync(ItemShort item)
	{
		return GetFromJsonAsync<Statistic>($"https://api.warframe.market/v1/items/{item.Slug}/statistics");
	}
	public Task<ItemCache> GetItemsCacheAsync()
	{
		return GetFromJsonAsync<ItemCache>($"https://api.warframe.market/v2/items");
	}
	public Task<string> GetItemsCacheTextAsync()
	{
		return GetStringAsync($"https://api.warframe.market/v2/items");
	}
}
