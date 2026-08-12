using Xunit;
using zms9110750.WarframeMarketApi;
using zms9110750.WarframeMarketApi.Models.Items;
using zms9110750.WarframeMarketApi.Models.Users;
using zms9110750.Warframe.Market.GUI.Services;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>IAppStateService：枚举↔字符串转换 + 状态属性</summary>
public class AppStateServiceTests
{
    private static IAppStateService Make()
    {
        return new AppState(new WarframeMarketClient(new HttpClient()));
    }

    [Fact]
    public void Lang_roundtrip()
    {
        var s = Make();
        Assert.Equal("zh-hans", s.LangToStr(Language.ZhHans));
        Assert.Equal("en", s.LangToStr(Language.En));
        Assert.Equal(Language.ZhHans, s.StrToLang("zh-hans"));
        Assert.Equal(Language.En, s.StrToLang("unknown")); // 未知回退 En
    }

    [Fact]
    public void Platform_roundtrip()
    {
        var s = Make();
        Assert.Equal("pc", s.PlatToStr(Platform.PC));
        Assert.Equal("ps4", s.PlatToStr(Platform.PS4));
        Assert.Equal(Platform.PS4, s.StrToPlat("ps4"));
        Assert.Equal(Platform.PC, s.StrToPlat("unknown")); // 未知回退 PC
    }

    [Fact]
    public void State_properties_settable()
    {
        var s = Make();
        s.ClickLink = true;
        s.CanWrite = true;
        s.VersionText = "数据日期 2026-08-12";
        Assert.True(s.ClickLink);
        Assert.True(s.CanWrite);
        Assert.Equal("数据日期 2026-08-12", s.VersionText);
    }
}
