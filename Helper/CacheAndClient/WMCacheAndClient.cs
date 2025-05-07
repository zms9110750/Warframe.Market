using Microsoft.Extensions.Caching.Memory;
using Nito.AsyncEx;
using System.Diagnostics;
using System.Xml.Linq;
using Warframe.Market.Extend;
using Warframe.Market.Helper.Abstract;
using Warframe.Market.Model.Items;
using Warframe.Market.Model.ItemsSet;
using Warframe.Market.Model.LocalItems;
using Warframe.Market.Model.Statistics;

namespace Warframe.Market.Helper.CacheAndClient
{
	/// <summary>
	/// WMCacheAndClient 类用于处理 Warframe 市场数据的缓存和客户端请求。
	/// </summary>
	public class WMCacheAndClient(ICacheLoacd loacd, IWMClient client, IMemoryCache cache) : IWMClient
	{
		private HashSet<Task> TasksToDispose { get; } = new HashSet<Task>();
		static MemoryCacheEntryOptions CacheEntryOptions { get; } = new MemoryCacheEntryOptions()
		{
			AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
			SlidingExpiration = TimeSpan.FromMinutes(1)
		};
		public async ValueTask DisposeAsync()
		{
			Trace.WriteLine("未完成的任务数量：" + TasksToDispose.Count(task => !task.IsCompletedSuccessfully));
			await foreach (var task in Task.WhenEach(TasksToDispose))
			{ 
			}
			Trace.WriteLine("所有任务已释放完成");
		}
		public Task<Item> GetItemAsync(ItemShort item, CancellationToken cancellationToken = default)
		{
			return cache.GetOrCreateAsync((item, nameof(GetItemAsync)), async (entry) =>
				{
					var itemDetails = await loacd.GetItemAsync(item, cancellationToken);
					if (itemDetails == null)
					{
						itemDetails = await client.GetItemAsync(item, cancellationToken);
						TasksToDispose.Add(loacd.SetFullItemAsync(itemDetails, cancellationToken));
					}
					entry.SetSize(1);
					return itemDetails;
				}, CacheEntryOptions)!;
		}
		public Task<ItemCache> GetItemsCacheAsync(CancellationToken cancellationToken = default)
		{
			return cache.GetOrCreateAsync(nameof(GetItemsCacheAsync), async (entry) =>
			{ 
				var itemsCache = await loacd.GetItemsCacheAsync(cancellationToken);
				if (itemsCache == null || itemsCache.ApiVersion != (await GetVersionAsync(cancellationToken)).ApiVersion)
				{
					itemsCache = await client.GetItemsCacheAsync(cancellationToken);
					var version = await GetVersionAsync(cancellationToken);
					TasksToDispose.Add(loacd.SetItemCacheAsync(itemsCache, version, cancellationToken));
				}
				entry.SetSize(itemsCache.Data.Length);
				return itemsCache;
			}, CacheEntryOptions)!;
		}

		public Task<ItemSet> GetItemSetAsync(ItemShort item, CancellationToken cancellationToken = default)
		{
			return cache.GetOrCreate((item, nameof(GetItemSetAsync)), (entry) =>
			{
				entry.SetSize(1);
				return client.GetItemSetAsync(item, cancellationToken);
			}, CacheEntryOptions)!;
		}
		public Task<Statistic> GetStatisticsAsync(ItemShort item, CancellationToken cancellationToken = default)
		{  
			return cache.GetOrCreateAsync((item, nameof(GetStatisticsAsync)), async (entry) =>
			{ ;
				var statistics = await loacd.GetStatisticsAsync(item, cancellationToken);
				if (statistics == null)
				{
					statistics = await client.GetStatisticsAsync(item, cancellationToken);
					TasksToDispose.Add(loacd.SetStatisticAsync(item, statistics, cancellationToken));
				}
				entry.SetSize(
					statistics.Payload.StatisticsLive.Day90.Length
					+ statistics.Payload.StatisticsClosed.Hour48.Length
					+ statistics.Payload.StatisticsLive.Day90.Length
					+ statistics.Payload.StatisticsClosed.Hour48.Length
					 );
				return statistics;
			}, CacheEntryOptions)!;
		}
		public Task<Model.Versions.Version> GetVersionAsync(CancellationToken cancellationToken = default)
		{
			return cache.GetOrCreate(nameof(GetVersionAsync), (entry) =>
			{
				entry.SetSize(1);
				return client.GetVersionAsync(cancellationToken);
			}, CacheEntryOptions)!;
		}
	}
}
