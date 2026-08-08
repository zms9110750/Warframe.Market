using Refit;

namespace zms9110750.Warframe.Market.GUI.Api;

/// <summary>
/// Gitee 发布版本查询（更新源：zms9110750/Warframe.Market）
/// </summary>
public interface IGitee
{
    /// <param name="direction">可选 desc / asc</param>
    [Get("/repos/{owner}/{repo}/releases")]
    Task<GiteeRelease[]> ReleasesAsync(
        string owner, string repo,
        [Query] int? page = default, [Query] int? per_page = default,
        [Query] string? direction = default,
        CancellationToken cancellation = default);
}

public record GiteeRelease(
    long Id,
    string TagName,
    string Name,
    string Body,
    DateTime CreatedAt,
    ReleaseAsset[] Assets);

public record ReleaseAsset(
    string BrowserDownloadUrl,
    string Name);
