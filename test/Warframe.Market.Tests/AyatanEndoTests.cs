using Xunit;
using zms9110750.WarframeMarketApi.Models.Items;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>阿耶檀识塑像 星星→内融核心 算法（Wiki 公式 + 表格数据验证）</summary>
public class AyatanEndoTests
{
    [Theory]
    [InlineData(2000, 0, 0, 4, 2000)]  // Anasa 无星
    [InlineData(2000, 0, 1, 4, 2306)]  // Anasa 1 青蓝
    [InlineData(2000, 1, 0, 4, 2363)]  // Anasa 1 琥珀
    [InlineData(2000, 0, 2, 4, 2625)]  // Anasa 2 青蓝
    [InlineData(2000, 1, 1, 4, 2688)]  // Anasa 1青1琥
    [InlineData(2000, 2, 0, 4, 2750)]  // Anasa 2 琥珀
    [InlineData(2000, 1, 2, 4, 3025)]  // Anasa 1琥2青
    [InlineData(2000, 2, 1, 4, 3094)]  // Anasa 2琥1青
    [InlineData(2000, 2, 2, 4, 3450)]  // Anasa 满星
    public void Anasa_matches_wiki_table(int b, int a, int c, int s, int expected)
    {
        Assert.Equal(expected, AyatanEndo.Compute(b, a, c, s));
    }

    [Theory]
    [InlineData(450, 0, 1, 3, 1000)]   // Zambuka/Chattraka/Hemakara 1 青蓝（M=3）
    [InlineData(450, 1, 2, 3, 2600)]   // Zambuka 满星（2青1琥）
    [InlineData(450, 1, 4, 5, 3000)]   // Kitha 满星（4青1琥，S=5，M=3）
    [InlineData(325, 0, 1, 3, 625)]    // Ayr 1 青蓝（M=2）
    [InlineData(325, 0, 3, 3, 1425)]   // Ayr 满星（3青蓝）
    [InlineData(650, 0, 1, 4, 1050)]   // Orta 1 青蓝（M=2）
    [InlineData(650, 1, 3, 4, 2700)]   // Orta 满星（1琥珀3青蓝）
    [InlineData(375, 0, 1, 3, 708)]    // Piv 1 青蓝
    [InlineData(375, 1, 2, 3, 1725)]   // Piv 满星
    [InlineData(400, 1, 2, 3, 1800)]   // Vaya 满星
    public void Other_sculptures_match_wiki_table(int b, int a, int c, int s, int expected)
    {
        Assert.Equal(expected, AyatanEndo.Compute(b, a, c, s));
    }

    [Fact]
    public void FromItem_uses_detail_fields()
    {
        // ayatan_orta_sculpture：baseEndo=650、maxAmberStars=1、maxCyanStars=3 → S=4
        var item = new Item(
            "id", "ayatan_orta_sculpture", "/Lotus/Types/Game/...", new HashSet<string> { "ayatan_sculpture" },
            0, null, null, 1, 3, 650, 2f, null,
            new Dictionary<Language, LanguagePake>(), "ayatan-orta-sculpture", true,
            false, null, null, null, null, null, null, null, null);

        Assert.Equal(2700, AyatanEndo.FromItem(item, 1, 3)); // 满星 1 琥珀 + 3 青蓝
        Assert.Equal(650, AyatanEndo.FromItem(item, 0, 0));   // 无星 = 650×1
        Assert.Null(AyatanEndo.FromItem(new Item(
            "id", "x", "/Lotus/Types/Game/...", new HashSet<string>(),
            0, null, null, 0, 0, 0, null, null,
            new Dictionary<Language, LanguagePake>(), "x", true,
            false, null, null, null, null, null, null, null, null), 1, 1)); // 非安魂（无槽位）→ null
    }

    [Fact]
    public void GetMultiplier_inference()
    {
        Assert.Equal(0.5, AyatanEndo.GetMultiplier(2000)); // Anasa
        Assert.Equal(3.0, AyatanEndo.GetMultiplier(450));  // Zambuka/Chattraka/Hemakara/Kitha（实测）
        Assert.Equal(2.0, AyatanEndo.GetMultiplier(325));  // 其余
    }

    [Fact]
    public void FromSlug_uses_catalog()
    {
        // Orta：B=650、S=4（1 琥珀 + 3 青蓝）
        Assert.Equal(650, AyatanEndo.FromSlug("ayatan_orta_sculpture", 0, 0));  // 无星
        Assert.Equal(2700, AyatanEndo.FromSlug("ayatan_orta_sculpture", 1, 3)); // 满星
        Assert.Equal(1050, AyatanEndo.FromSlug("ayatan_orta_sculpture", 0, 1)); // 1 青蓝
        Assert.Null(AyatanEndo.FromSlug("galvanized_steel", 0, 0));             // 非塑像
    }

    [Fact]
    public void RangeFromSlug_gives_min_to_max()
    {
        var r = AyatanEndo.RangeFromSlug("ayatan_anasa_sculpture");
        Assert.NotNull(r);
        Assert.Equal(2000, r.Value.Min);  // 无星
        Assert.Equal(3450, r.Value.Max);  // 满星 2琥珀2青蓝

        var k = AyatanEndo.RangeFromSlug("ayatan_kitha_sculpture");
        Assert.NotNull(k);
        Assert.Equal(450, k.Value.Min);
        Assert.Equal(3000, k.Value.Max);  // 满星 1琥珀4青蓝

        Assert.Null(AyatanEndo.RangeFromSlug("secura_dual_cestra"));
    }
}
