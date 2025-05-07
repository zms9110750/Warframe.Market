using Warframe.Market.Model.Items;
using Warframe.Market.Model.ItemsSet;
using Warframe.Market.Model.LocalItems;
using Warframe.Market.Model.Statistics;

namespace Warframe.Market.Helper.Abstract;
public interface IWMClient : IAsyncDisposable
{
	Task<ItemCache> GetItemsCacheAsync(CancellationToken cancellation = default);
	Task<Item> GetItemAsync(ItemShort item, CancellationToken cancellation = default);
	Task<ItemSet> GetItemSetAsync(ItemShort item, CancellationToken cancellation = default);
	Task<Statistic> GetStatisticsAsync(ItemShort item, CancellationToken cancellation = default);
	Task<Model.Versions.Version> GetVersionAsync(CancellationToken cancellation = default);
}