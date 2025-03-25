using Autofac;
using Newtonsoft.Json.Linq;
using System.IO.Pipelines;
using System.Xml.Linq;
using Warframe.Market.Helper;
using Warframe.Market.Model.LocalItems;

namespace Warframe.Market.Configuration;

public static class AutofacCreatOfWarframeMarket
{
	private const string _configFileName = "config.json";
	public static async Task<ContainerBuilder> Creat()
	{
		var builder = new ContainerBuilder();
		WMClient client = new WMClient();
		AutofacConfig config = await LoadConfig();
		Directory.CreateDirectory(config.StatisticPath);
		Directory.CreateDirectory(Path.GetDirectoryName(config.CacheFileName)!);
		var cacheTask = LoadCache(config, client);
		IConfigParser<XElement, ArcanePackage[]> xmlParse = new XmlToArcanePackage();
		var package = xmlParse.Parse(XElement.Parse(XmlToArcanePackage.GetArcaneXml()));
		builder.RegisterInstance(package).As<IEnumerable<ArcanePackage>>();
		foreach (var item in package)
		{
			item.WMClient = client;
			builder.RegisterInstance(item).Named<ArcanePackage>(item.Name);
		}
		var cache = await cacheTask;
		cache.PriceCachePath = config.StatisticPath;
		builder.RegisterInstance(client);
		builder.RegisterInstance(cache);
		foreach (var item in cache)
		{
			item.Value.WMClient = client;
			builder.RegisterInstance(item.Value).Named<ItemShort>(item.Key);
			builder.RegisterInstance(item.Value.Slug).Named<string>(item.Key);
		}
		foreach (var item in package)
		{
			item.ItemCache = cache;
		}
		return builder;
	}
	private static async Task<AutofacConfig> LoadConfig()
	{
		AutofacConfig config;
		FileInfo configFile = new FileInfo(_configFileName);
		// 如果配置文件存在，加载并返回
		if (configFile.Exists)
		{
			using var configReader = configFile.OpenText();
			await using var jsonReader = new JsonTextReader(configReader);
			config = (await JObject.LoadAsync(jsonReader)).ToObject<AutofacConfig>()!;
		}
		else
		{
			config = new AutofacConfig();

			await using var configWriter = configFile.CreateText();
			JsonTextWriter jsonWrite = new JsonTextWriter(configWriter);
			await JObject.FromObject(config).WriteToAsync(jsonWrite);
		}
		return config;
	}
	private static async Task<ItemCache> LoadCache(AutofacConfig config, WMClient client)
	{
		FileInfo cacheInfo = new FileInfo(config.CacheFileName);
		// 如果缓存文件存在且版本对就序列化。否则从服务器获取
		if (cacheInfo.Exists)
		{
			var version = client.GetVersionAsync();
			await using var cacheRead = cacheInfo.OpenRead();
			var pipeReader = PipeReader.Create(cacheRead);
			var jsonRead = new Utf8JsonReader((await pipeReader.ReadAtLeastAsync(256)).Buffer);
			while (jsonRead.Read() && jsonRead.TokenType != JsonTokenType.PropertyName)
			{
			}
			if (jsonRead.GetString() == config.VersionKey && jsonRead.Read() && jsonRead.GetString() == (await version).ApiVersion)
			{
				using StreamReader cacjeReader = new StreamReader(cacheRead);
				await using var jsonReader = new JsonTextReader(cacjeReader);
				cacheRead.Position = 0;
				return (await JObject.LoadAsync(jsonReader)).ToObject<ItemCache>()!;
			}
		}
		var cacheText = await client.GetItemsCacheTextAsync();
		await using (var cacheWrite = cacheInfo.CreateText())
		{
			await cacheWrite.WriteAsync(cacheText);
		}
		return JObject.Parse(cacheText).ToObject<ItemCache>()!;
	}
}
