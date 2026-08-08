using Serilog;

namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>
/// 语言包下载：从 wfm-localization 仓库（42bytes-team/wfm-localization）拉取 locales/{lang}.json，
/// 保存到本地 <see cref="LocalesDir"/>。下载后的语言会用于缓存（物品 i18n 含该语言）。
/// </summary>
public class LocalizationDownloadService
{
    public const string Owner = "42bytes-team";
    public const string Repo = "wfm-localization";

    /// <summary>本地语言包目录（程序数据目录下 locales/）</summary>
    public static string LocalesDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WarframeMarket", "locales");

    private readonly HttpClient _http;

    public LocalizationDownloadService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>下载并保存某语言包；返回是否成功</summary>
    public async Task<bool> DownloadAsync(string language, CancellationToken ct = default)
    {
        try
        {
            Directory.CreateDirectory(LocalesDir);
            var url = $"https://raw.githubusercontent.com/{Owner}/{Repo}/master/locales/{language}.json";
            Log.Information("下载语言包: {Lang} <- {Url}", language, url);
            var bytes = await _http.GetByteArrayAsync(url, ct);
            var path = Path.Combine(LocalesDir, $"{language}.json");
            await File.WriteAllBytesAsync(path, bytes, ct);
            Log.Information("语言包已保存: {Path} ({Length} 字节)", path, bytes.Length);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "语言包下载失败: {Lang}", language);
            return false;
        }
    }

    /// <summary>本地是否已存在该语言包</summary>
    public static bool IsDownloaded(string language)
    {
        return File.Exists(Path.Combine(LocalesDir, $"{language}.json"));
    }
}
