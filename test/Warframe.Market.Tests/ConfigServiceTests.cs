using Xunit;
using zms9110750.Warframe.Market.GUI.Data;
using zms9110750.Warframe.Market.GUI.Services;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>IConfigService：配置读写（临时目录注入，不碰真实 %LocalAppData%）</summary>
public class ConfigServiceTests
{
    private static string TempDir()
    {
        return Path.Combine(Path.GetTempPath(), $"wm-cfg-{Guid.NewGuid():N}");
    }

    [Fact]
    public void LoadAppConfig_creates_default_when_missing()
    {
        IConfigService svc = new ConfigService(TempDir());
        var cfg = svc.LoadAppConfig();

        Assert.Equal("zh-hans", cfg.DefaultLanguage);
        Assert.Equal("pc", cfg.DefaultPlatform);
        Assert.True(cfg.DefaultCrossplay);
    }

    [Fact]
    public void SaveAppConfig_then_load_roundtrips()
    {
        var dir = TempDir();
        IConfigService svc = new ConfigService(dir);
        svc.SaveAppConfig(new AppConfig {
            DefaultLanguage = "zh-hans",
            DefaultPlatform = "ps4",
            DefaultCrossplay = false,
            DownloadedLanguages = new List<string> { "ru", "ko" },
        });

        var loaded = new ConfigService(dir).LoadAppConfig();
        Assert.Equal("ps4", loaded.DefaultPlatform);
        Assert.False(loaded.DefaultCrossplay);
        Assert.Equal(2, loaded.DownloadedLanguages.Count);
    }

    [Fact]
    public void LoadArcaneConfig_writes_default_copy_and_reads()
    {
        IConfigService svc = new ConfigService(TempDir());
        var packs = svc.LoadArcaneConfig();

        // 从测试 bin 的 赋能包配置.yaml 复制默认；应有包
        Assert.NotEmpty(packs);
    }

    [Fact]
    public void Corrupted_config_falls_back_to_default()
    {
        var dir = TempDir();
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "config.yaml"), "::: 坏 yaml :::");

        IConfigService svc = new ConfigService(dir);
        var cfg = svc.LoadAppConfig(); // 不抛，回退默认
        Assert.Equal("pc", cfg.DefaultPlatform);
    }
}
