using Autofac.Features.AttributeFilters;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using Warframe.Market.Helper.Abstract;
using Warframe.Market.Helper.AutofacModule;
using Warframe.Market.Model.Items;
using Warframe.Market.Model.ItemsSet;
using Warframe.Market.Model.LocalItems;
using Warframe.Market.Model.Statistics;

namespace Warframe.Market.Helper.CacheAndClient;

public class WClient([KeyFilter(ResilientHttpModule.Key)] HttpClient http) : IWMClient
{
	public ValueTask DisposeAsync()
	{
		return default;
	}

	public async Task<Item> GetItemAsync(ItemShort item, CancellationToken cancellation = default)
	{
		return JObject.Parse(await http.GetStringAsync($"https://api.warframe.market/v2/item/{item.Slug}", cancellation)).ToObject<Item>()!;
	}

	public async Task<ItemCache> GetItemsCacheAsync(CancellationToken cancellation = default)
	{
		return JObject.Parse(await http.GetStringAsync("https://api.warframe.market/v2/items", cancellation)).ToObject<ItemCache>()!;
	}

	public async Task<ItemSet> GetItemSetAsync(ItemShort item, CancellationToken cancellation = default)
	{
		return JObject.Parse(await http.GetStringAsync($"https://api.warframe.market/v2/item/{item.Slug}/set", cancellation)).ToObject<ItemSet>()!;
	}

	public async Task<Statistic> GetStatisticsAsync(ItemShort item, CancellationToken cancellation = default)
	{
		return JObject.Parse(await http.GetStringAsync($"https://api.warframe.market/v1/items/{item.Slug}/statistics", cancellation)).ToObject<Statistic>()!;
	}

	public async Task<Model.Versions.Version> GetVersionAsync(CancellationToken cancellation = default)
	{
		return JObject.Parse(await http.GetStringAsync("https://api.warframe.market/v2/versions", cancellation)).ToObject<Model.Versions.Version>()!;
	}
}
