using System.Text.Json;
using Serilog;

namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>
/// 子类别中文本地化：读取官方 wfm-localization 的 zh-hans.json（app.subtype.* 键），
/// 复制到输出目录（csproj Content）。不再硬编码翻译。
/// </summary>
public static class SubtypeLocalization
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase);
    private static bool _loaded;

    public static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Resources", "i18n", "zh-hans.json");
            if (File.Exists(path))
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    // app.subtype.intact → "完整"；跳过 app.subtype.intact.en 变体
                    if (prop.Name.StartsWith("app.subtype.", StringComparison.Ordinal)
                        && !prop.Name.EndsWith(".en", StringComparison.Ordinal))
                    {
                        var key = prop.Name["app.subtype.".Length..];
                        Map[key] = prop.Value.GetString() ?? key;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "子类别本地化加载失败（回退显示英文原值）");
        }

        _loaded = true;
    }

    /// <summary>子类别中文名；未找到回退英文原值</summary>
    public static string Get(string? subtype)
    {
        if (subtype == null)
        {
            return "";
        }

        EnsureLoaded();
        return Map.TryGetValue(subtype, out var v) ? v : subtype;
    }
}
