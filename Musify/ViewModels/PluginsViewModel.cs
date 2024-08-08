using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Musify.Helpers;
using Musify.Models;
using Musify.Plugins.Abstract;
using Musify.Plugins.Enums;
using Musify.Plugins.Models;
using Musify.Services;
using Musify.Views;
using System.ComponentModel;

namespace Musify.ViewModels;

public partial class PluginsViewModel : ObservableObject
{
    readonly ILogger<PluginsViewModel> logger;
    readonly MainView mainView;

    public Config Config { get; }
    public PluginManager PluginManager { get; }

    public PluginsViewModel(
        ILogger<PluginsViewModel> logger,
        Config config,
        PluginManager pluginManager,
        MainView mainView)
    {
        this.logger = logger;
        this.Config = config;
        this.PluginManager = pluginManager;
        this.mainView = mainView;

        Plugins = new()
        {
            KeySelector = Config.Plugins.Sorting switch
            {
                Sorting.Default => null,
                Sorting.Title => plugin => plugin.Name,
                Sorting.Artist => plugin => plugin.Author,
                Sorting.Duration => plugin => plugin.LastUpdatedAt,
                _ => null
            },
            Descending = Config.Plugins.SortDescending,
            Filter = plugin => true
        };
        Config.Plugins.PropertyChanged += OnConfigPropertyChanged;

        pluginManager.SubscribeLoadContext(
            loadContext => Plugins.Add(loadContext.Manifest),
            loadContext => Plugins.Remove(loadContext.Manifest));

        logger.LogInformation("[PluginsViewModel-.ctor] PluginsViewModel has been initialized");
    }



    void OnConfigPropertyChanged(object? _, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "Sorting":
                Plugins.KeySelector = Config.Plugins.Sorting switch
                {
                    Sorting.Default => null,
                    Sorting.Title => plugin => plugin.Name,
                    Sorting.Artist => plugin => plugin.Author,
                    Sorting.Duration => plugin => plugin.LastUpdatedAt,
                    _ => null
                };
                break;
            case "SortDescending":
                Plugins.Descending = Config.Plugins.SortDescending;
                break;
        }
        logger.LogInformation("[DownloadsViewModel-OnViewOptionsPropertyChanged] Reordered search results");
    }


    public ObservableRangeCollection<Manifest> Plugins { get; }



    [RelayCommand]
    void Import()
    {
        logger.LogInformation("[PluginsViewModel-.ctor] Plugin was imported from file");
    }
}