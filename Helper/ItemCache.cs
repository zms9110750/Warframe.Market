using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text;
using Warframe.Market.Model.ItemType;

namespace Warframe.Market.Helper;

public class ItemCache
{
	public Item this[string key]
	{
		get
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(key);
			return Parse(KeyOfId.GetValueOrDefault(key)
			   ?? KeyOfEnName.GetValueOrDefault(key)
			   ?? KeyOfZhName.GetValueOrDefault(key)
			   ?? KeyOfSlug.GetValueOrDefault(key)
			   ?? throw new ArgumentException("这个key在字典中不存在:" + key));
		}
	}
	IReadOnlyDictionary<string, JObject> KeyOfId { get; }
	IReadOnlyDictionary<string, JObject> KeyOfEnName { get; }
	IReadOnlyDictionary<string, JObject> KeyOfZhName { get; }
	IReadOnlyDictionary<string, JObject> KeyOfSlug { get; }
	public ItemCache(JObject jobject)
	{
		JArray jArray = (JArray)jobject["data"]!;
		KeyOfId = jArray.ToDictionary(s => s.SelectToken("id")!.Value<string>(), s => (JObject)s)!;
		KeyOfEnName = jArray.ToDictionary(s => s.SelectToken("i18n.en.name")!.Value<string>(), s => (JObject)s)!;
		KeyOfZhName = jArray.ToDictionary(s => s.SelectToken("i18n.zh-hans.name")!.Value<string>(), s => (JObject)s)!;
		KeyOfSlug = jArray.ToDictionary(s => s.SelectToken("slug")!.Value<string>(), s => (JObject)s)!;
	}
	public ItemCache() : this(JObject.Parse(ReadStringFromAssembly())) { }
	private static string ReadStringFromAssembly()
	{
		using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Warframe.Market.Configuration.items.json");
		using StreamReader reader = new StreamReader(stream!, Encoding.UTF8);
		return reader.ReadToEnd();
	}
	public static async Task<ItemCache> CreatFromAPI(WMClient client)
	{
		return new ItemCache(JObject.Parse(await client.GetStringAsync("items")));
	}
	public Task<Item> ParseFromAPI(WMClient client, string key)
	{
		return client.GetItemAsync(this[key].Slug);
	}
	public static Item Parse(JToken jobject)
	{
		if (jobject["tags"]?.ToObject<HashSet<string>>() is not HashSet<string> set)
		{
			return jobject.ToObject<Item>()!;
		}
		else if (set.Contains("riven_mod"))
		{
			return jobject.ToObject<RivenMOD>()!;
		}
		else if (set.Contains("mod"))
		{
			return jobject.ToObject<MOD>()!;
		}
		else if (set.Contains("fish"))
		{
			return jobject.ToObject<Fish>()!;
		}
		else if (set.Contains("relic"))
		{
			return jobject.ToObject<Relic>()!;
		}
		else if (set.Contains("prime"))
		{
			return jobject.ToObject<PrimeComponent>()!;
		}
		else if (set.Contains("arcane_enhancement"))
		{
			return jobject.ToObject<ArcaneEnhancement>()!;
		}
		else if (set.Contains("ayatan_sculpture"))
		{
			return jobject.ToObject<AyatanSculpture>()!;
		}
		else if (set.Contains("component") || set.Contains("set") || set.Contains("modular"))
		{
			return jobject.ToObject<Component>()!;
		}
		else if (set.Contains("weapon"))
		{
			return jobject.ToObject<Equipment>()!;
		}
		return jobject.ToObject<Item>()!;
	}
}
//		key	局部变量和参数在“[Exception]”调用堆栈帧中不可用。 若要获取这些内容，请将调试器配置为在引发异常时停止，然后重新运行该方案。	string
