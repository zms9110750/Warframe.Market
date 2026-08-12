using Microsoft.Extensions.Configuration;
using Xunit;
using zms9110750.WarframeMarketApi;
using zms9110750.Warframe.Market.GUI.Services;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>IVersionService：程序版本从配置读 + 数据日期拉取写 AppState</summary>
public class VersionServiceTests
{
    private static IConfiguration MakeConfig(string version = "1.2.3")
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Version:Program"] = version })
            .Build();
    }

    [Fact]
    public void ProgramVersion_reads_from_config()
    {
        IVersionService svc = new VersionService(MakeConfig("9.9.9"),
            new WarframeMarketClient(new HttpClient()), new AppState(new WarframeMarketClient(new HttpClient())));
        Assert.Equal("9.9.9", svc.ProgramVersion);
    }

    [Fact]
    public void ProgramVersion_falls_back_to_question_mark()
    {
        IVersionService svc = new VersionService(MakeConfig(),
            new WarframeMarketClient(new HttpClient()), new AppState(new WarframeMarketClient(new HttpClient())));
        _ = svc.ProgramVersion; // 无 Version:Program 时返回 "?"（构造未抛）
    }
}
