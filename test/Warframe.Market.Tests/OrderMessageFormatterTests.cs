using Xunit;
using zms9110750.WarframeMarketApi;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>
/// 私聊消息格式化测试：ICU MessageFormat 子集解析 + 多语言模板 + 语言规则
/// （中文目标用中文模板；非中文用英文模板 + 末尾追加 对方语言物品名 <=> 价格）
/// </summary>
public class OrderMessageFormatterTests
{
    [Fact]
    public void Template_contains_all_locales()
    {
        // order_clipboard_template.json 应含 en 与 zh-hans 等键
        Assert.False(string.IsNullOrWhiteSpace(OrderMessageFormatter.GetTemplate("en")));
        Assert.False(string.IsNullOrWhiteSpace(OrderMessageFormatter.GetTemplate("zh-hans")));
    }

    [Fact]
    public void Format_simple_placeholder()
    {
        var msg = OrderMessageFormatter.Format("{ingameName} 你好", new Dictionary<string, string> { ["ingameName"] = "Alice" });
        Assert.Equal("Alice 你好", msg);
    }

    [Fact]
    public void Format_select_buy_branch()
    {
        // 英文模板的 action select：buy 分支
        var msg = OrderMessageFormatter.Format(
            "I want to {action, select, buy {buy} sell {sell} other {}}: {itemName}",
            new Dictionary<string, string> { ["action"] = "buy", ["itemName"] = "Rhino Prime Set" });
        Assert.Contains("I want to buy:", msg);
        Assert.Contains("Rhino Prime Set", msg);
    }

    [Fact]
    public void Format_select_missing_key_falls_back_to_other()
    {
        // key 缺失（undefined）→ 走 other 分支（模板常见 undefined {} 空分支）
        var msg = OrderMessageFormatter.Format(
            "{perTrade, select, undefined {} other {x{perTrade} }}{itemName}",
            new Dictionary<string, string> { ["itemName"] = "Steel" });
        Assert.StartsWith("Steel", msg.TrimStart()); // undefined → 空
    }

    [Fact]
    public void BuildMessage_chinese_target_uses_chinese_template()
    {
        var msg = OrderMessageFormatter.BuildMessage(
            locale: "zh-hans", action: "buy", ingameName: "买家",
            itemName: "Rhino Prime 一套", itemNameLocalized: null,
            perTrade: null, subtype: null, modRank: null, ayatan: null, price: 300);
        Assert.Contains("你好", msg);
        Assert.Contains("Rhino Prime 一套", msg);
        Assert.Contains("300", msg);
        Assert.DoesNotContain("<=>", msg); // 中文目标不加 i18n 追加
    }

    [Fact]
    public void BuildMessage_english_target_uses_trading_emoji_no_arrow()
    {
        // 非中文目标：英文模板 + :trading:/:platinum: 表情段（不再追加 <=> 格式）
        var msg = OrderMessageFormatter.BuildMessage(
            locale: "fr", action: "sell", ingameName: "Vendeur",
            itemName: "Rhino Prime Set", itemNameLocalized: "Ensemble Rhino Prime",
            perTrade: 2, subtype: null, modRank: null, ayatan: null, price: 200);
        Assert.Contains("Hi!", msg);
        Assert.Contains("Rhino Prime Set :trading: 200 :platinum:", msg);
        Assert.DoesNotContain("<=>", msg);
    }

    [Fact]
    public void BuildMessage_perTrade_one_is_omitted_but_greater_than_one_shown()
    {
        // x1（默认）不显示；x2+ 显示（对方库存更多则买更多，数量未必）
        var m1 = OrderMessageFormatter.BuildMessage(
            locale: "en", action: "buy", ingameName: "Buyer",
            itemName: "Wisp Prime Chassis Blueprint", itemNameLocalized: null,
            perTrade: 1, subtype: null, modRank: null, ayatan: null, price: 13);
        Assert.DoesNotContain("x1", m1);

        var m2 = OrderMessageFormatter.BuildMessage(
            locale: "en", action: "buy", ingameName: "Buyer",
            itemName: "Wisp Prime Chassis Blueprint", itemNameLocalized: null,
            perTrade: 2, subtype: null, modRank: null, ayatan: null, price: 13);
        Assert.Contains("x2", m2);
    }

    [Fact]
    public void BuildMessage_rank_and_subtype_are_rendered()
    {
        var msg = OrderMessageFormatter.BuildMessage(
            locale: "en", action: "buy", ingameName: "Buyer",
            itemName: "Blind Rage", itemNameLocalized: null,
            perTrade: null, subtype: null, modRank: 10, ayatan: null, price: 150);
        Assert.Contains("(rank 10)", msg);
    }
}
