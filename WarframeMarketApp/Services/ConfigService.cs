using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using WarframeMarketApp.Data;

namespace WarframeMarketApp.Services;

/// <summary>
/// 配置管理。读写 YAML 文件。
/// </summary>
public class ConfigService
{
	private readonly string _appDir;
	private readonly string _appConfigPath;
	private readonly string _arcaneConfigPath;

	private static readonly IDeserializer YamlDeser = new DeserializerBuilder()
		.IgnoreUnmatchedProperties()
		.WithNamingConvention(NullNamingConvention.Instance)
		.Build();

	private static readonly ISerializer YamlSer = new SerializerBuilder()
		.WithNamingConvention(NullNamingConvention.Instance)
		.Build();

	public ConfigService()
	{
		_appDir = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"WarframeMarket");
		Directory.CreateDirectory(_appDir);
		_appConfigPath = Path.Combine(_appDir, "config.yaml");
		_arcaneConfigPath = Path.Combine(_appDir, "赋能包配置.yaml");
	}

	/// <summary>读取应用配置</summary>
	public AppConfig LoadAppConfig()
	{
		if (!File.Exists(_appConfigPath))
		{
			var def = new AppConfig();
			SaveAppConfig(def);
			return def;
		}
		var yaml = File.ReadAllText(_appConfigPath);
		return YamlDeser.Deserialize<AppConfig>(yaml) ?? new AppConfig();
	}

	public void SaveAppConfig(AppConfig config)
	{
		var yaml = YamlSer.Serialize(config);
		File.WriteAllText(_appConfigPath, yaml);
	}

	/// <summary>读取赋能包配置</summary>
	public ArcanePackConfig[] LoadArcaneConfig()
	{
		if (!File.Exists(_arcaneConfigPath))
		{
			// 从内置资源复制默认配置
			WriteDefaultArcaneConfig();
		}
		var yaml = File.ReadAllText(_arcaneConfigPath);
		var result = YamlDeser.Deserialize<ArcaneConfigRoot>(yaml);
		return result?.赋能包配置 ?? Array.Empty<ArcanePackConfig>();
	}

	private static void WriteDefaultArcaneConfig()
	{
		// 从程序目录复制默认配置
		var src = Path.Combine(AppContext.BaseDirectory, "赋能包配置.yaml");
		if (File.Exists(src))
		{
			var dst = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"WarframeMarket", "赋能包配置.yaml");
			File.Copy(src, dst, true);
		}
	}

	private class ArcaneConfigRoot
	{
		public ArcanePackConfig[] 赋能包配置 { get; set; } = Array.Empty<ArcanePackConfig>();
	}
}
