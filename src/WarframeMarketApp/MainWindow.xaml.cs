using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Windows;
using zms9110750.WarframeMarketApi;
using WarframeMarketApp.Data;
using WarframeMarketApp.Services;

namespace WarframeMarketApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        LogSetup.Configure();
        Log.Information("=== 会话启动 ===");

        try
        {
            InitializeComponent();

            Width = SystemParameters.PrimaryScreenWidth / 3 * 2;
            Height = SystemParameters.PrimaryScreenHeight / 3 * 2;

            var dbPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WarframeMarket", "wfm.db");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dbPath)!);

            var wfm = new WarframeMarketClient
            {
                Crossplay = true,
                Language = zms9110750.WarframeMarketApi.Models.Items.Language.ZhHans,
                Platform = zms9110750.WarframeMarketApi.Models.Users.Platform.PC,
            };

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddMasaBlazor();
            serviceCollection.AddMemoryCache();
            serviceCollection.AddSingleton(wfm);
            serviceCollection.AddDbContext<WfmDbContext>(o =>
                o.UseSqlite($"Data Source={dbPath}"));
            serviceCollection.AddSingleton<AppState>();
            serviceCollection.AddSingleton<CacheService>();
            serviceCollection.AddSingleton<PersistentStorage>();
            serviceCollection.AddSingleton<ItemsService>();
            serviceCollection.AddTransient<ConfigService>();
            serviceCollection.AddSingleton<ArcaneService>();

#if DEBUG
            serviceCollection.AddBlazorWebViewDeveloperTools();
#endif

            Resources.Add("services", serviceCollection.BuildServiceProvider());

            // 初始化数据库 + 创建缺失的表
            using (var scope = ((IServiceProvider)Resources["services"]).CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WfmDbContext>();
                db.Database.EnsureCreated();
                // 确保 QuickReplyItems 表存在（旧库可能没有）
                db.Database.ExecuteSqlRaw(
                    "CREATE TABLE IF NOT EXISTS QuickReplies (Id INTEGER PRIMARY KEY AUTOINCREMENT, Text TEXT NOT NULL, SortOrder INTEGER NOT NULL)");
            }

            // 延迟清理（不阻塞 UI）
            var cacheService = ((IServiceProvider)Resources["services"]).GetRequiredService<CacheService>();
            _ = cacheService.StartupCleanupAsync();
        }
        catch (Exception ex)
        {
            System.IO.File.WriteAllText("crash.log",
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}");
            Close();
        }
    }
}
