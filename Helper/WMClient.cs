using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Warframe.Market.Model;
using Warframe.Market.Model.ItemType;
using Warframe.Market.Model.Statistics;

namespace Warframe.Market.Helper;

public class WMClient : HttpClient
{
	MemoryCache Cache { get; } = new MemoryCache(new MemoryCacheOptions { SizeLimit = 1024 * 1024 * 4, ExpirationScanFrequency = TimeSpan.FromSeconds(5) });
	AsyncRetryPolicy Http429RetryPolicy { get; } = Policy
			.Handle<HttpRequestException>(ex => ex.StatusCode == HttpStatusCode.TooManyRequests)
		 .WaitAndRetryAsync(retryCount: 5, sleepDurationProvider: (retryCount) => TimeSpan.FromMilliseconds(Random.Shared.Next(500) + (500 * retryCount))
		 , onRetry: (ex, delay, retryCount, context) =>
		 { 
			 Console.WriteLine($"[WARNING] 触发429重试机制 | 第 {retryCount} 次重试 | " +
							   $"等待 {delay.TotalSeconds:F1} 秒 | 异常消息: {ex. Message}"); 
		 });
	public WMClient(TimeSpan? interval = null)
	{
		BaseAddress = new Uri("https://api.warframe.market/v2/");
		DefaultRequestHeaders.Add("Language", "zh-hans");
		Throttle = new ThrottleGate(interval ?? TimeSpan.FromSeconds(1.0 / 3));
	}
	ThrottleGate Throttle { get; }
	public new async Task<string> GetStringAsync(string url)
	{
		await Throttle;
		var s = await Http429RetryPolicy.ExecuteAsync(() => base.GetStringAsync(url)); 
		return s;
	}
	public async Task<Item> GetItemAsync(string slug)
	{
		if (Cache.TryGetValue<Item>(slug, out var result))
		{
			return result!;
		}
		JObject js = (JObject)JObject.Parse(await GetStringAsync($"item/{slug}"))["data"]!;
		if (js.ContainsKey("setRoot"))
		{
			result = ParseComponent(js);
		}
		else if (js.ContainsKey("reqMasteryRank"))
		{
			result = js.ToObject<Equipment>()!;
		}
		else
		{
			result = ItemCache.Parse(js)!;
		}
		Cache.Set(slug, result, new MemoryCacheEntryOptions { Size = js.ToString().Length });
		return result;
	}
	public async Task<ItemSet> GetItemSetAsync(string slug)
	{
		if (Cache.TryGetValue<ItemSet>(slug, out var result))
		{
			return result!;
		}
		JArray js = (JArray)JObject.Parse(await GetStringAsync($"item/{slug}/set")!)["data"]!["items"]!;
		result = new ItemSet(js.Select(s => ParseComponent((JObject)s)));
		foreach (var item in result)
		{
			Cache.Set(item.Slug, item, new MemoryCacheEntryOptions { Size = item.ToString().Length });
			Cache.Set(item.Slug + "/set", result);
		}
		return result;
	}
	public async Task<MarketData> GetStatisticsAsync(string slug)
	{ 
		if (Cache.TryGetValue<MarketData>(slug + "/Statistics", out var result))
		{
			return result!;
		}
		JObject js = JObject.Parse(await GetStringAsync($"https://api.warframe.market/v1/items/{slug}/statistics"));
		result = js.ToObject<MarketData>()!;
		Cache.Set(slug, result, new MemoryCacheEntryOptions { Size = js.ToString().Length });
		return result;
	}
	private static Component ParseComponent(JObject js)
	{
		if (js.ContainsKey("ducats"))
		{
			return js.ToObject<PrimeComponent>()!;
		}
		else if (js.ContainsKey("subtypes"))
		{
			return js.ToObject<CraftedComponent>()!;
		}
		else
		{
			return js.ToObject<Component>()!;
		}
	}
}
