using Warframe.Market.Model.Items;
using Warframe.Market.Model.LocalItems;
using Warframe.Market.Model.Statistics;
namespace Warframe.Market.Helper.Abstract;

using Warframe.Market.Model.Versions;
public interface ICacheLoacd : IWMClient
{
	Task SetItemCacheAsync(ItemCache cache, Version version, CancellationToken cancellation = default);
	Task SetFullItemAsync(Item item, CancellationToken cancellation = default);
	Task SetFullItemAsync(IEnumerable<Item> item, CancellationToken cancellation = default);
	Task SetStatisticAsync(ItemShort itemShort, Statistic statistic, CancellationToken cancellation = default);
}