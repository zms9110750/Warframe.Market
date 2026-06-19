using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using zms9110750.WarframeMarketApi;
using WarframeMarketApp.Data;
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

            var dbPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WarframeMarket", "wfm.db");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dbPath)!);

            var wfm = new WarframeMarketClient();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddMasaBlazor();
            serviceCollection.AddSingleton(wfm);
            serviceCollection.AddDbContext<WfmDbContext>(o =>
                o.UseSqlite($"Data Source={dbPath}"));
            serviceCollection.AddSingleton<AppState>();
            serviceCollection.AddTransient<CacheService>();
            serviceCollection.AddTransient<LocalCacheService>();
            serviceCollection.AddTransient<ConfigService>();
            serviceCollection.AddTransient<ItemsCacheService>();

#if DEBUG
            serviceCollection.AddBlazorWebViewDeveloperTools();
#endif

            Resources.Add("services", serviceCollection.BuildServiceProvider());
        }
        catch (Exception ex)
        {
            System.IO.File.WriteAllText("crash.log",
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}");
            Close();
        }
    }
}
