using FusionCacheReference;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System.Windows;
using WarframeMarketQuery.API;
using WarframeMarketQuery.Extension;
using WarframeMarketQueryWPF.Api;

namespace WarframeMarketQueryWPF;


public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 将窗口宽和高设置为屏幕宽高的一半
        Width = SystemParameters.PrimaryScreenWidth / 3 * 2;
        Height = SystemParameters.PrimaryScreenHeight / 3 * 2;


        var serviceCollection = new ServiceCollection();
        serviceCollection.AddWpfBlazorWebView();
        serviceCollection.AddMasaBlazor();

#if DEBUG
        serviceCollection.AddBlazorWebViewDeveloperTools();
#endif
        serviceCollection.AddFusionCacheAndSqliteCache();
        serviceCollection
            .AddWarframeMarketClient()
            .AddWarframeMarketProgramServices();
        serviceCollection.AddRefitClient<IGitee>(new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer(IWarframeMarketApiV1.V1options) }).ConfigureHttpClient(http => http.BaseAddress = new Uri("https://gitee.com/api/v5"));

        Resources.Add("services", serviceCollection.BuildServiceProvider());
    }
}
