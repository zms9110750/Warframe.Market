using System.Windows;

namespace WarframeMarketApp;

public partial class App : Application
{
    public App()
    {
        DispatcherUnhandledException += (_, e) =>
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("App 未处理异常");
            var cur = e.Exception;
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

            // Masa Blazor JS 清理在 WebView 中会炸（removeEventListener/getAttributeNames），
            // 这种异常不可恢复但可以忽略不崩
            if (e.Exception is System.Reflection.TargetInvocationException tie
                && tie.InnerException is Microsoft.JSInterop.JSException)
            {
                e.Handled = true;
                return;
            }

            e.Handled = true;
            Current.Shutdown();
        };
    }
}
