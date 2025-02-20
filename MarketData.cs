using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.RateLimit;
using Polly.Timeout;
using Polly.Wrap;
using System.Net;
using System.Net.Http.Json;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
namespace Warframe.Market;
public record MarketData([property: JsonPropertyName("payload"), JsonProperty("payload")] MarketDataPayload Payload)
{
	public static IAsyncEnumerable<(string itemName, int? rank, double price)> GetAveragePrice(IEnumerable<string> itemNames)
	{
		var client = new HttpClient { BaseAddress = new Uri("https://api.warframe.market/v1/items/") };
		var http429RetryPolicy = Policy
			.Handle<HttpRequestException>(ex => ex.StatusCode == HttpStatusCode.TooManyRequests)
			.WaitAndRetryAsync(5, (retryCount) => TimeSpan.FromMilliseconds(Random.Shared.Next(500, 2000) + 500 * retryCount));

		return itemNames
			.ToObservable()
			.Zip(Observable.Interval(TimeSpan.FromMilliseconds(50)), (item, _) => item)
			.Select(item => Observable.FromAsync(() =>
				   http429RetryPolicy.ExecuteAsync(() =>
					 client.GetFromJsonAsync<MarketData>($"{item.ToLower().Replace(" ", "_")}/statistics")
						.ContinueWith(response => (item, response.Result!.Payload.GetReferencePrice())) )))
			.Merge(maxConcurrent: 5)
			.SelectMany(result => result.Item2.Select(s => (result.Item1, s.Item1, s.Item2)))
			.ToAsyncEnumerable();
	}
}