// 构建初始容器
using Autofac;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using Warframe.Market.Configuration;
using Warframe.Market.Helper;
using Warframe.Market.Model.LocalItems;
using System.Text.RegularExpressions;


var build = await AutofacCreatOfWarframeMarket.Creat();
var container = build.Build();
var p2 = container.Resolve<IEnumerable<ArcanePackage>>();
List<MyStruct> list = [];
foreach (var item in p2)
{
	foreach (var item2 in item.SelectMany(s => s))
	{
		var a = container.ResolveNamed<ItemShort>(item2);
		Warframe.Market.Model.Statistics.Statistic statistic = await a.GetPriceAsync();
		double v = statistic.GetReferencePrice(a.PriceFilterMaxRank());
		MyStruct my = new MyStruct()
		{
			名称 = item2,
			概率 = item.GetProbability(item2),
			赋能包 = item.Name,
			交易量 = statistic.Payload.StatisticsClosed.Day90.Sum(s => s.ModRank == 0 ? s.Volume / ItemShort.SyntheticConsumption[a.MaxRank.Value] : s.Volume) / 90.0,
			满级 = v,
			零级价格 = v / ItemShort.SyntheticConsumption[a.MaxRank.Value],
			概率x零级x100 = v / ItemShort.SyntheticConsumption[a.MaxRank.Value] * item.GetProbability(item2)
		};
		list.Add(my);
	}
}
HashSet<MyStruct> set = [];
foreach (var item in list.OrderByDescending(s => s.满级).Take(6))
{
	set.Add(item);
}
foreach (var item in list.OrderByDescending(s => s.概率x零级x100).Take(6))
{
	set.Add(item);
}
foreach (var item in list.OrderByDescending(s => s.交易量).Take(6))
{
	set.Add(item);
}
foreach (var item2 in list.GroupBy(s => s.赋能包))
{
	foreach (var item in item2.OrderByDescending(s => s.概率x零级x100).Take(2))
	{
		set.Add(item);
	}
	foreach (var item in item2.OrderByDescending(s => s.交易量).Take(2))
	{
		set.Add(item);
	}
}
foreach (var item in set.OrderByDescending(s => s.概率x零级x100))
{
	item.概率 = Math.Round(item.概率 * 100, 1);
	item.概率x零级x100 = Math.Round(item.概率x零级x100 * 100);
	item.满级 = Math.Round(item.满级);
	item.零级价格 = Math.Round(item.零级价格, 1);
	item.交易量 = Math.Round(item.交易量, 1);



	var e = Facilitate.SerializeXml(item);
	e.Name = e.Attribute("名称").Value[3..];
	e.SetAttributeValue("名称", null);
	string pattern = @"(([^ ]* ){3}[^ ]*) ";
	 var s= Regex.Replace(e.ToString(), pattern, "$1\n\t");

	Console.WriteLine(s);
}
public class MyStruct
{
	[XmlAttribute] public string 名称 { get; set; }
	[XmlAttribute] public double 概率 { get; set; }
	[XmlAttribute] public double 满级 { get; set; }
	[XmlIgnore] public double 零级价格 { get; set; }
	[XmlAttribute] public double 交易量 { get; set; }
	[XmlAttribute] public double 概率x零级x100 { get; set; }
	[XmlAttribute] public string 赋能包 { get; set; }
}
public static class Facilitate
{
	private static Process proc = Process.GetCurrentProcess();
	/// <summary>
	/// 获取本程序使用的内存
	/// </summary>
	public static long UsedMemory
	{
		get
		{
			proc.Refresh();
			return proc.NonpagedSystemMemorySize64;
		}
	}

	/// <summary>
	/// 将对象xml序列化为字符串
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="value"></param>
	/// <returns></returns>
	public static string Serialize<T>(T value)
	{
		ArgumentNullException.ThrowIfNull(value, nameof(value));
		XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces([XmlQualifiedName.Empty]);
		XmlSerializer xml = new XmlSerializer(typeof(T));
		using StringWriter sw = new StringWriter();
		xml.Serialize(sw, value, namespaces);
		return sw.ToString();
	}
	/// <summary>
	/// 将对象序列化为xml节点
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="value"></param>
	/// <returns></returns>
	public static XElement SerializeXml(object value)
	{
		ArgumentNullException.ThrowIfNull(value, nameof(value));
		XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces([XmlQualifiedName.Empty]);
		XmlSerializer xml = new XmlSerializer(value.GetType());
		using MemoryStream ms = new MemoryStream();
		xml.Serialize(ms, value, namespaces);
		ms.Position = 0;
		return XElement.Load(ms);
	}
	/// <summary>
	/// 将xml字符串反序列化
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="value"></param>
	/// <returns></returns>
	public static T? DeSerialize<T>(string value)
	{
		XmlSerializer xml = new XmlSerializer(typeof(T));
		StringReader sr = new StringReader(value);
		using var xmlread = XmlReader.Create(sr);
		return (T?)xml.Deserialize(xmlread);
	}
	public const string QQ1622 = "A:\\下载\\QQ应用数据\\1622989459\\Image\\Group2";
	public const string QQ250 = "A:\\下载\\QQ应用数据\\2503372027\\Image\\Group2";
	public static void CleanQQImage(string path)
	{
		DirectoryInfo directory = new DirectoryInfo(path);
		if (directory.Name != "Group2" || directory.Parent!.Name != "Image")
		{
			return;
		}
		foreach (var item in directory.EnumerateFiles("*", SearchOption.AllDirectories))
		{
			var t = DateTime.Now - item.LastAccessTime;
			if (t.TotalDays > 7 && t.TotalDays * item.Length > 1024 * 1024)
			{
				Console.WriteLine(t.Days + "," + item.Length);
				item.Delete();
			}
		}
	}
	/// <summary>
	/// 复制源目录中的文件到目标目录，并保持相对路径。  
	/// 仅会复制小于等于10MB的文件，并确保目标文件不存在或大小不同。
	/// </summary>
	/// <param name="sourceDirectory">源目录的路径。</param>
	/// <param name="targetDirectory">目标目录的路径。</param>
	public static IEnumerable<string> CopyFilesWithRelativePath(string sourceDirectory, string targetDirectory)
	{
		Directory.CreateDirectory(targetDirectory);
		foreach (string dir in Directory.EnumerateDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
		{
			string relativePath = Path.GetRelativePath(sourceDirectory, dir);
			string targetPath = Path.Combine(targetDirectory, relativePath.Replace(Path.DirectorySeparatorChar, '.'));
			var files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly)
				.Take(10)
				.Select(f => new FileInfo(f))
				.OrderBy(f => Math.Abs(f.Length - 10 * 1024 * 1024))
				.ToArray();
			if (files.Length > 0)
			{
				var file = files.FirstOrDefault(f => Extension.Contains(Path.GetExtension(f.Name)), files[0]);
				string ext = Path.GetExtension(file.Name);
				string targetFile = targetPath + ext;
				if (!File.Exists(targetFile) || new FileInfo(targetFile).Length != file.Length)
				{
					File.Copy(file.FullName, targetFile, true);
				}
				yield return targetFile;
			}
		}
	}
	readonly static string[] Extension = [
".jpg",
".JPG",
".png",
".jpeg",
".PNG" , ];
}