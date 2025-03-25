using Warframe.Market.Model.LocalItems;
using Warframe.Market.Model.Statistics;

namespace Warframe.Market.Configuration;

public class AutofacConfig(string CacheFileName = $"{nameof(ItemCache)}\\items.json", string StatisticPath = $"{nameof(ItemCache)}\\{nameof(Statistic)}", string VersionKey = "apiVersion", TimeSpan CacheExpiry = default)
{
	public string CacheFileName { get; set; } = CacheFileName;
	public string StatisticPath { get; set; } = StatisticPath;
	public string VersionKey { get; set; } = VersionKey;
	public TimeSpan CacheExpiry { get; set; } = CacheExpiry == default ? TimeSpan.FromHours(2) : CacheExpiry;
}