using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Musify.Helpers;
using Musify.Models;
using Musify.Plugins.Enums;
using Musify.Plugins.Models;
using Musify.Services;
using Musify.Views;
using System.ComponentModel;
using Windows.Storage.Pickers;
using Windows.Storage;
using Musify.Enums;
using Microsoft.UI.Xaml.Controls;
using Musify.Plugins.Exceptions;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Extensions.DependencyInjection;
using Musify.Plugins;

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

        PluginBundles = new()
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
            Filter = plugin =>
                plugin.Name.Contains(Query, StringComparison.InvariantCultureIgnoreCase) &&
                (
                    (Config.Plugins.ShowInstalled) ||
                    (false) // Config.Plugins.ShowAvailable
                ) &&
                (
                    (Config.Plugins.ShowOfKindPlatformSupport && plugin.PluginKinds.Contains(PluginKind.PlatformSupport)) ||
                    (Config.Plugins.ShowOfKindMetadata && plugin.PluginKinds.Contains(PluginKind.Metadata))
                )
        };
        Config.Plugins.PropertyChanged += OnConfigPropertyChanged;

        pluginManager.SubscribeLoadContext(
            loadContext => PluginBundles.Add(loadContext.Manifest),
            loadContext => PluginBundles.Remove(loadContext.Manifest));

        logger.LogInformation("[PluginsViewModel-.ctor] PluginsViewModel has been initialized");
    }



    void OnConfigPropertyChanged(object? _, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "Sorting":
                PluginBundles.KeySelector = Config.Plugins.Sorting switch
                {
                    Sorting.Default => null,
                    Sorting.Title => plugin => plugin.Name,
                    Sorting.Artist => plugin => plugin.Author,
                    Sorting.Duration => plugin => plugin.LastUpdatedAt,
                    _ => null
                };
                logger.LogInformation("[PluginsViewModel-OnViewOptionsPropertyChanged] Reordered plugin bundles");
                break;
            case "SortDescending":
                PluginBundles.Descending = Config.Plugins.SortDescending;
                logger.LogInformation("[PluginsViewModel-OnViewOptionsPropertyChanged] Reordered plugin bundles");
                break;
            case "ShowOfKindPlatformSupport":
            case "ShowOfKindMetadata":
            case "ShowInstalled":
            case "ShowAvailable":
                PluginBundles.Refresh();
                logger.LogInformation("[PluginsViewModel-OnViewOptionsPropertyChanged] Refreshed plugin bundles");
                break;
        }
    }


    public ObservableRangeCollection<Manifest> PluginBundles { get; }


    [ObservableProperty]
    Manifest? selectedPluginBundle = null;

    async partial void OnSelectedPluginBundleChanged(
        Manifest? value)
    {
        if (value is null)
            return;

        logger.LogInformation("[PluginsViewModel-OnSelectedSearchResultChanged] Creating PluginInfoViewModel...");

        PluginInfoViewModel viewModel = App.Provider.GetRequiredService<PluginInfoViewModel>();
        viewModel.PluginBundle = value;

        if (await mainView.AlertAsync(new PluginInfoView(viewModel), $"Plugin: {value.Name}", "Close", "Remove") != ContentDialogResult.Primary)
        {
            SelectedPluginBundle = null;
            logger.LogInformation("[PluginsViewModel-OnSelectedPluginBundleChanged] Plugin info was shown: {name}", value.Name);
            return;
        }

        try
        {
            PluginLoadContext loadContext = PluginManager.LoadedContexts.Distinct().FirstOrDefault(loadContext => loadContext.Manifest == value) ?? throw new("Could not find any plugin load context which matches the requested manifest.");

            PluginManager.Unload(loadContext);
            File.Delete(loadContext.Location);

            SelectedPluginBundle = null;

            mainView.ShowNotification("Success!", $"Removed plugin: {value.Name}.", NotificationLevel.Success);
            logger.LogInformation("[PluginsViewModel-OnSelectedPluginBundleChanged] Removed plugin: {name}", value.Name);
        }
        catch (Exception ex)
        {
            SelectedPluginBundle = null;

            mainView.ShowNotification("Something went wrong!", $"Failed to remove plugin: {value.Name}.", NotificationLevel.Error, ex.ToFormattedString());
            logger.LogError(ex, "[PluginsViewModel-OnSelectedPluginBundleChanged] Failed to remove plugin: {name}", value.Name);
        }
    }


    [ObservableProperty]
    string query = string.Empty;

    partial void OnQueryChanged(
        string value)
    {
        PluginBundles.Refresh();
        logger.LogInformation("[PluginsViewModel-OnQueryChanged] Refreshed plugin bundles");
    }


    public async Task TryLoadAsync(
        string path)
    {
        string pluginFileName = Path.GetFileNameWithoutExtension(path);
        try
        {
            await PluginManager.LoadAsync(path);
            mainView.ShowNotification("Success!", $"Loaded plugin: {pluginFileName}.", NotificationLevel.Success);
        }
        catch (PluginNotLoadedException ex)
        {
            mainView.ShowNotification("Warning!", $"Could not load plugin: {pluginFileName}.", NotificationLevel.Warning, async () =>
            {
                if (await mainView.AlertAsync(
                    new()
                    {
                        Content = ex.ToFormattedString("Resetting the config may be able to fix this isuee.\nDo you want to reset the config for this plugin and restart the app?"),
                        Title = "Warning!",
                        CloseButtonText = "No",
                        PrimaryButtonText = "Yes"
                    }) != ContentDialogResult.Primary)
                    return;

                Config.Plugins.Configs.Remove(ex.PluginType!.Name);

                mainView.Close();
                AppInstance.Restart(null);
            });
            logger.LogWarning(ex, "[AppStartupHandler-LoadPlugins] Could not load plugin: {pluginFileName}: {exception}", pluginFileName, ex.Message);
        }
        catch (Exception ex)
        {
            mainView.ShowNotification("Warning!", $"Could not load plugin: {pluginFileName}.", NotificationLevel.Warning, ex.ToFormattedString());
            logger.LogWarning(ex, "[AppStartupHandler-LoadPlugins] Could not load plugin: {pluginFileName}: {exception}", pluginFileName, ex.Message);
        }
    }

    [RelayCommand]
    async Task ImportAsync()
    {
        FileOpenPicker picker = new()
        {
            CommitButtonText = "Import plugin",
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            SettingsIdentifier = "Import plugin",
        };
        picker.FileTypeFilter.Add(".mfy");
        mainView.Initialize(picker);

        StorageFile? file = await picker.PickSingleFileAsync();
        logger.LogInformation("[PluginsViewModel-ImportAsync] File open picker was shown");

        if (file is null)
            return;

        string destination = Path.Combine(PluginManager.PluginsDirectory, file.Name);
        if (File.Exists(destination))
        {
            mainView.ShowNotification("Something went wrong!", "Failed to import plugin.", NotificationLevel.Error, "It looks like a plugin with the same name is already imported. Please first uninstall that plugin.");
            logger.LogError(new Exception("It looks like a plugin with the same name is already imported. Please first uninstall that plugin."), "Failed to import plugin: {fileName}", file.Name);
            return;
        }

        File.Copy(file.Path, destination);
        await TryLoadAsync(destination);

        logger.LogInformation("[PluginsViewModel-ImportAsync] Plugin was imported from file");
    }
}