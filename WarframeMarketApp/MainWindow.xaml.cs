using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using zms9110750.WarframeMarketApi;
using WarframeMarketApp.Services;

namespace WarframeMarketApp;

public partial class MainWindow : Window
{
    public MainWindow()
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
}
