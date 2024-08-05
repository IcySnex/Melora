using GeniusAPI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Musify.Helpers;
using Musify.Models;
using Musify.Plugins.Abstract;
using Musify.Services;
using Musify.ViewModels;
using Musify.Views;
using Serilog;

namespace Musify;

public partial class App : Application
{
    readonly IHost host;

    public static IServiceProvider Provider { get; private set; } = default!;
    public static InMemorySink Sink { get; private set; } = new();

    public static string? Parameter { get; set; }

    public App()
    {
        host = Host.CreateDefaultBuilder()
            .UseSerilog((context, configuration) =>
            {
                configuration.WriteTo.Debug();
                configuration.WriteTo.File(@"Logs\Log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10);
                configuration.WriteTo.Sink(Sink);
            })
            .ConfigureServices((context, services) =>
            {
                // Configuration
                services.AddSingleton(provider =>
                {
                    if (!File.Exists(Config.ConfigFilepath))
                        return new();

                    string json = File.ReadAllText(Config.ConfigFilepath);

                    JsonConverter converter = provider.GetRequiredService<JsonConverter>();
                    return converter.ToObject<Config>(json) ?? new();
                });

                // Add services
                services.AddSingleton<AppStartupHandler>();
                services.AddSingleton<PluginManager>();
                services.AddSingleton<Navigation>();
                services.AddSingleton<JsonConverter>();
                services.AddSingleton<MediaEncoder>();
                services.AddSingleton(provider => new GeniusClient(
                    provider.GetRequiredService<Config>().Lyrics.GeniusAccessToken,
                    provider.GetRequiredService<ILogger<GeniusClient>>()));

                // Add ViewModels and MainView
                services.AddSingleton<HomeViewModel>();
                services.AddSingleton<SettingsViewModel>();

                services.AddTransient<PlatformViewModel>();

                services.AddSingleton<LyricsViewModel>();
                services.AddTransient<LyricsInfoViewModel>();

                services.AddSingleton<DownloadsViewModel>();
                services.AddSingleton<DownloadableTrackInfoViewModel>();

                services.AddSingleton<MainView>();

            })
            .Build();
        Provider = host.Services;

        InitializeComponent();
    }


    protected override void OnLaunched(LaunchActivatedEventArgs args) =>
        Provider.GetRequiredService<AppStartupHandler>();

}