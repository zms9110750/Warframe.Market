using System.Text.Json;
using zms9110750.WarframeMarketApi.Models.Statistics;

var slugs = new[] {
    "galvanized_hell", "galvanized_diffusion", "galvanized_aptitude", "galvanized_scope",
    "galvanized_acceleration", "galvanized_crosshairs", "galvanized_shot", "galvanized_savvy",
    "galvanized_chamber", "galvanized_steel", "galvanized_reflex", "galvanized_elementalist",
    "blind_rage", "transient_fortitude", "narrow_minded", "high_voltage", "shell_shock", "voltaic_strike",
};

var dir = Path.Combine("..", "..", "test", "Resources", "statistics");
var options = new JsonSerializerOptions {
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
};

Console.WriteLine($"{"slug",-24} | {"满级参考价",10} | {"0级参考价",10}");
foreach (var slug in slugs)
{
    var path = Path.Combine(dir, slug + ".json");
    if (!File.Exists(path))
    {
        Console.WriteLine($"{slug,-24} | 文件缺失");
        continue;
    }
    var stat = JsonSerializer.Deserialize<Statistic>(File.ReadAllText(path), options);
    var max = stat?.GetMaxReferencePrice();
    var base_ = stat?.GetReferencePrice();
    Console.WriteLine($"{slug,-24} | {max,10:F1} | {base_,10:F1}");
}
