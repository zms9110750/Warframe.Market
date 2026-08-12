using Xunit;
using zms9110750.Warframe.Market.GUI.Services;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>ILocalizationDownloadService：IsDownloaded（临时目录）+ DownloadAsync（fake handler）</summary>
public class LocalizationDownloadServiceTests
{
    [Fact]
    public async Task Download_async_saves_and_is_downloaded()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"wm-loc-{Guid.NewGuid():N}");
        var handler = new FakeDownloadHandler("{\"app.subtype.intact\":\"完整\"}");
        ILocalizationDownloadService svc = new LocalizationDownloadService(new HttpClient(handler), dir);

        Assert.False(svc.IsDownloaded("zh-hans"));
        Assert.True(await svc.DownloadAsync("zh-hans"));
        Assert.True(svc.IsDownloaded("zh-hans"));
        Assert.Contains("完整", File.ReadAllText(Path.Combine(dir, "zh-hans.json")));
    }

    private sealed class FakeDownloadHandler : HttpMessageHandler
    {
        private readonly byte[] _body;

        public FakeDownloadHandler(string body)
        {
            _body = System.Text.Encoding.UTF8.GetBytes(body);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) {
                Content = new ByteArrayContent(_body),
            });
        }
    }
}
