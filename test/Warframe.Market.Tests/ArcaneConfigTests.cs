using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>
/// 赋能包配置 yaml 解析（复刻 GUI 的 ArcaneModels/ConfigService；Subtypes 为标量 string）
/// </summary>
public class ArcaneConfigTests
{
    private sealed class ArcaneConfigRoot
    {
        public ArcanePackConfig[] 赋能包配置 { get; set; } = [];
    }

    private sealed class ArcanePackConfig
    {
        public string Name { get; set; } = "";
        public ArcaneQualityGroup[] Items { get; set; } = [];
    }

    private sealed class ArcaneQualityGroup
    {
        public string Subtypes { get; set; } = "";
        public double Quality { get; set; }
        public string[] Items { get; set; } = [];
    }

    [Fact]
    public void Arcane_pack_yaml_deserializes()
    {
        var deser = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithNamingConvention(NullNamingConvention.Instance)
            .Build();

        var yaml = File.ReadAllText(Data.File("arcanepack", "赋能包配置.yaml"));
        var root = deser.Deserialize<ArcaneConfigRoot>(yaml);

        Assert.NotNull(root);
        Assert.NotEmpty(root!.赋能包配置);
        Assert.Equal(9, root.赋能包配置.Length);
        foreach (var pack in root.赋能包配置)
        {
            Assert.False(string.IsNullOrEmpty(pack.Name));
            Assert.NotEmpty(pack.Items);
            foreach (var group in pack.Items)
            {
                Assert.False(string.IsNullOrEmpty(group.Subtypes)); // 标量 string 解析成功
                Assert.True(group.Quality > 0);
                Assert.NotEmpty(group.Items);
            }
        }
    }
}
