namespace zms9110750.Warframe.Market.GUI.Services;

/// <summary>语言包下载（wfm-localization）与本地存在性检查</summary>
public interface ILocalizationDownloadService
{
    /// <summary>下载并保存某语言包；返回是否成功</summary>
    Task<bool> DownloadAsync(string language, CancellationToken ct = default);

    /// <summary>本地是否已存在该语言包</summary>
    bool IsDownloaded(string language);
}
