using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using zms9110750.WarframeMarketApi;
using WarframeMarketApp.Services;

namespace WarframeMarketApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        try
        {
            InitializeComponent();

            Width = SystemParameters.PrimaryScreenWidth / 3 * 2;
            Height = SystemParameters.PrimaryScreenHeight / 3 * 2;

            var wfm = new WarframeMarketClient();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddMasaBlazor();
            serviceCollection.AddSingleton(wfm);
            serviceCollection.AddSingleton<AppState>();

#if DEBUG
            serviceCollection.AddBlazorWebViewDeveloperTools();
#endif

            Resources.Add("services", serviceCollection.BuildServiceProvider());
        }
        catch (Exception ex)
        {
            WriteCrashLog("MainWindow 构造", ex);
            Close();
        }
    }

    private static void WriteCrashLog(string phase, Exception ex)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"阶段: {phase}");
        var cur = ex;
        int depth = 0;
        while (cur != null)
        {
            sb.AppendLine($"--- {depth} ---");
            sb.AppendLine($"类型: {cur.GetType().FullName}");
            sb.AppendLine($"消息: {cur.Message}");
            sb.AppendLine($"堆栈: {cur.StackTrace}");
            cur = cur.InnerException;
            depth++;
        }
        System.IO.File.WriteAllText("crash.log", sb.ToString());
    }
}
