using Refit;

namespace WarframeMarketQueryWPF.Api;

public interface IGitee
{
    /// <summary>
    /// 
    /// </summary> 
    /// <remarks>direction 可选 desc 和 asc</remarks>
    /// <returns></returns>
    [Get("/repos/{owner}/{repo}/releases")]
    public Task<GiteeRelease[]> Releases(string owner, string repo, [Query] int? page = default, [Query] int? per_page = default, [Query] string? direction = default);
}
public record GiteeRelease(long Id, string TagName, string Name, string Body, DateTime CreatedAt, ReleaseAsset[] Assets);

public record ReleaseAsset(string BrowserDownloadUrl, string Name);