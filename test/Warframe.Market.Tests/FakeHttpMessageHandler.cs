using System.Net;
using System.Text;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>
/// 捕获请求 URL，并从 test/ 下备份的本地 JSON 假数据文件返回响应。
/// 按 URL 路径（不含查询串）精确匹配。
/// </summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, string> _responses = new();

    /// <summary>最近一次请求的完整 URI</summary>
    public Uri? LastRequestUri { get; private set; }

    /// <summary>所有已发出的请求 URI</summary>
    public List<Uri> RequestUris { get; } = new();

    /// <summary>把 URL 路径映射到本地 JSON 备份文件</summary>
    public void Map(string path, string jsonFilePath)
    {
        _responses[path] = jsonFilePath;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequestUri = request.RequestUri;
        RequestUris.Add(request.RequestUri!);

        var path = request.RequestUri!.AbsolutePath;
        if (_responses.TryGetValue(path, out var file))
        {
            var json = File.ReadAllText(file);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
                RequestMessage = request,
            });
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) {
            RequestMessage = request,
        });
    }
}

/// <summary>
/// 指向 test/Resources/ 下备份的 JSON 数据文件（构建时复制到输出目录）
/// </summary>
internal static class Data
{
    /// <param name="resource">资源英文文件夹名，如 items / orders / users</param>
    /// <param name="file">文件夹内的 JSON 文件名</param>
    public static string File(string resource, string file)
    {
        return System.IO.Path.Combine(AppContext.BaseDirectory, "Resources", resource, file);
    }
}
