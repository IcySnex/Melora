﻿using GeniusAPI;
using Melora.Helpers;
using Melora.Models;
using Melora.Services;
using Melora.ViewModels;
using Melora.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Serilog;

namespace Melora;

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
                services.AddSingleton<UpdateManager>();
                services.AddSingleton<Navigation>();
                services.AddSingleton<JsonConverter>();
                services.AddSingleton<MediaEncoder>();
                services.AddSingleton(provider => new GeniusClient(
                    provider.GetRequiredService<Config>().Lyrics.GeniusAccessToken,
                    provider.GetRequiredService<ILogger<GeniusClient>>()));

                // Add ViewModels and MainView
                services.AddSingleton<MainView>();

                services.AddSingleton<DownloadableTrackInfoViewModel>();
                services.AddTransient<LyricsInfoViewModel>();
                services.AddSingleton<PluginBundleInfoViewModel>();

                services.AddSingleton<HomeViewModel>();
                services.AddTransient<PlatformViewModel>();
                services.AddSingleton<LyricsViewModel>();
                services.AddSingleton<DownloadsViewModel>();
                services.AddSingleton<PluginBundlesViewModel>();
                services.AddSingleton<SettingsViewModel>();
            })
            .Build();
        Provider = host.Services;

        InitializeComponent();
    }


    protected override void OnLaunched(LaunchActivatedEventArgs args) =>
        Provider.GetRequiredService<AppStartupHandler>();

}