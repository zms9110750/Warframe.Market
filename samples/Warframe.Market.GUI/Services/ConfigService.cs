using System.IO;
using zms9110750.Warframe.Market.GUI.Data;
using zms9110750.WarframeMarketApi.Models.Arcane;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>
/// 配置管理：config.yaml（应用配置）/ 赋能包配置.yaml（业务数据源）/ ui-config.yaml（UI 定制）
/// </summary>
public class ConfigService
{
    private readonly string _appDir;
    private readonly string _appConfigPath;
    private readonly string _arcaneConfigPath;
    private readonly string _uiConfigPath;

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
        _uiConfigPath = Path.Combine(AppContext.BaseDirectory, "ui-config.yaml");
    }

    public AppConfig LoadAppConfig()
    {
        if (!File.Exists(_appConfigPath))
        {
            var def = new AppConfig();
            SaveAppConfig(def);
            return def;
        }
        return YamlDeser.Deserialize<AppConfig>(File.ReadAllText(_appConfigPath)) ?? new AppConfig();
    }

    public void SaveAppConfig(AppConfig config)
    {
        File.WriteAllText(_appConfigPath, YamlSer.Serialize(config));
    }

    public ArcanePackConfig[] LoadArcaneConfig()
    {
        if (!File.Exists(_arcaneConfigPath))
        {
            WriteDefaultArcaneConfig();
        }

        var yaml = File.ReadAllText(_arcaneConfigPath);
        return YamlDeser.Deserialize<ArcaneConfigRoot>(yaml)?.赋能包配置 ?? [];
    }

    private void WriteDefaultArcaneConfig()
    {
        var src = Path.Combine(AppContext.BaseDirectory, "赋能包配置.yaml");
        if (File.Exists(src))
        {
            File.Copy(src, _arcaneConfigPath, true);
        }
    }

    public UIConfig LoadUIConfig()
    {
        if (!File.Exists(_uiConfigPath))
        {
            return new UIConfig();
        }

        return YamlDeser.Deserialize<UIConfig>(File.ReadAllText(_uiConfigPath)) ?? new UIConfig();
    }

    private class ArcaneConfigRoot
    {
        public ArcanePackConfig[] 赋能包配置 { get; set; } = [];
    }
}
