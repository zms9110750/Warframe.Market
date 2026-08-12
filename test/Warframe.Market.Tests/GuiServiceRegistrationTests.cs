using System.Text.RegularExpressions;
using Xunit;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>
/// GUI 服务注册完整性测试：扫描所有 razor 页面的 [Inject] 类型，
/// 断言 Program.cs 的 DI 注册全部覆盖——防止"页面注入某服务但忘记注册"导致的
/// 运行时渲染期 NullReferenceException（渲染期异常不经过全局异常兜底，表现为白屏/报错）。
/// </summary>
public class GuiServiceRegistrationTests
{
    private static string FindGuiRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "samples", "Warframe.Market.GUI")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return Path.Combine(dir.FullName, "samples", "Warframe.Market.GUI");
    }

    [Fact]
    public void All_razor_injected_services_are_registered_in_program()
    {
        var guiRoot = FindGuiRoot();
        var program = File.ReadAllText(Path.Combine(guiRoot, "Program.cs"));

        // 提取所有 [Inject] 的类型（接口或具体类）
        var injectTypes = Directory
            .GetFiles(guiRoot, "*.razor", SearchOption.AllDirectories)
            .SelectMany(f => Regex.Matches(
                    File.ReadAllText(f),
                    @"\[Inject\]\s+(?:private\s+|public\s+)?([A-Za-z_][\w\.<>]*?)\s+\w+\s*\{")
                .Cast<Match>())
            .Select(m => m.Groups[1].Value)
            .Where(t => t != "IJSRuntime") // bUnit/PhotinoX 自动提供，不需显式注册
            .Distinct()
            .OrderBy(t => t)
            .ToArray();

        Assert.NotEmpty(injectTypes);

        var missing = injectTypes
            .Where(t => !program.Contains($"AddSingleton<{t}>") && !program.Contains($"AddSingleton<{t},"))
            // 以下类型由框架/工厂方式注册，非显式 AddSingleton<T>：
            // IHttpClientFactory ← AddHttpClient("wfm")；IFusionCache ← AddFusionCache()；
            // WarframeMarketClient ← AddSingleton(sp => new WarframeMarketClient(...))
            .Where(t => !t.EndsWith("IHttpClientFactory") && !t.EndsWith("IFusionCache") && !t.EndsWith("WarframeMarketClient"))
            .ToArray();

        Assert.True(missing.Length == 0,
            $"以下 [Inject] 类型未在 Program.cs 注册（会导致渲染期 NRE）: {string.Join(", ", missing)}");
    }
}
