using Xunit;
using zms9110750.Warframe.Market.GUI.Services;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>子类别本地化：从官方 wfm-localization zh-hans.json（复制到输出目录）读取，不硬编码</summary>
public class SubtypeLocalizationTests
{
    [Fact]
    public void Official_subtypes_translate_to_chinese()
    {
        // csproj 始终复制 Resources/i18n/zh-hans.json 到输出目录
        Assert.Equal("完整", SubtypeLocalization.Get("intact"));
        Assert.Equal("光辉", SubtypeLocalization.Get("radiant"));
        Assert.Equal("优良", SubtypeLocalization.Get("exceptional"));
        Assert.Equal("无瑕", SubtypeLocalization.Get("flawless"));
        Assert.Equal("蓝图", SubtypeLocalization.Get("blueprint"));
        Assert.Equal("成品", SubtypeLocalization.Get("crafted"));
        Assert.Equal("已揭示", SubtypeLocalization.Get("revealed"));
        Assert.Equal("未揭示", SubtypeLocalization.Get("unrevealed"));
        Assert.Equal("（基本级）", SubtypeLocalization.Get("basic"));   // 官方用括号形式
        Assert.Equal("（装饰级）", SubtypeLocalization.Get("adorned"));
        Assert.Equal("（华丽级）", SubtypeLocalization.Get("magnificent"));
        Assert.Equal("（小）", SubtypeLocalization.Get("small"));
    }

    [Fact]
    public void Unknown_subtype_falls_back_to_original()
    {
        Assert.Equal("some_unknown_subtype", SubtypeLocalization.Get("some_unknown_subtype"));
        Assert.Equal("", SubtypeLocalization.Get(null));
    }
}
