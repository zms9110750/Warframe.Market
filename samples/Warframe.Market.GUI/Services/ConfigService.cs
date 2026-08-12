using System.IO;
using zms9110750.Warframe.Market.GUI.Data;
using zms9110750.WarframeMarketApi.Models.Arcane;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>
/// 配置管理：config.yaml（应用配置）/ 赋能包配置.yaml（业务数据源）/ ui-config.yaml（UI 定制）
/// </summary>
public class ConfigService : IConfigService
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

    public ConfigService(string? baseDir = null)
    {
        _appDir = baseDir ?? Path.Combine(
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
            Log.Information("ConfigService 首次创建默认配置: {Path}", _appConfigPath);
            var def = new AppConfig();
            SaveAppConfig(def);
            return def;
        }
        try
        {
            return YamlDeser.Deserialize<AppConfig>(File.ReadAllText(_appConfigPath)) ?? new AppConfig();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "ConfigService 配置解析失败，使用默认值: {Path}", _appConfigPath);
            return new AppConfig();
        }
    }

    public void SaveAppConfig(AppConfig config)
    {
        File.WriteAllText(_appConfigPath, YamlSer.Serialize(config));
    }

    public ArcanePackConfig[] LoadArcaneConfig()
    {
        if (!File.Exists(_arcaneConfigPath))
        {
            Log.Information("ConfigService 首次写入赋能包默认配置: {Path}", _arcaneConfigPath);
            WriteDefaultArcaneConfig();
        }

        try
        {
            var yaml = File.ReadAllText(_arcaneConfigPath);
            return YamlDeser.Deserialize<ArcaneConfigRoot>(yaml)?.赋能包配置 ?? [];
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "ConfigService 赋能包配置解析失败，返回空包列表: {Path}", _arcaneConfigPath);
            return [];
        }
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
