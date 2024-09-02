using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Melora.Enums;
using Melora.Helpers;
using Melora.Models;
using Melora.Plugins;
using Melora.Plugins.Abstract;
using Melora.Plugins.Enums;
using Melora.Plugins.Exceptions;
using Melora.Plugins.Models;
using Melora.Services;
using Melora.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using System.ComponentModel;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Melora.ViewModels;

public partial class PluginBundlesViewModel : ObservableObject
{
    readonly ILogger<PluginBundlesViewModel> logger;
    readonly MainView mainView;

    public Config Config { get; }
    public PluginManager PluginManager { get; }

    public PluginBundlesViewModel(
        ILogger<PluginBundlesViewModel> logger,
        Config config,
        PluginManager pluginManager,
        MainView mainView)
    {
        this.logger = logger;
        this.Config = config;
        this.PluginManager = pluginManager;
        this.mainView = mainView;

        Manifests = new()
        {
            KeySelector = Config.PluginBundles.Sorting switch
            {
                Sorting.Default => null,
                Sorting.Title => plugin => plugin.Name,
                Sorting.Artist => plugin => plugin.Author,
                Sorting.Duration => plugin => plugin.LastUpdatedAt,
                _ => null
            },
            Descending = Config.PluginBundles.SortDescending,
            Filter = plugin =>
                plugin.Name.Contains(Query, StringComparison.InvariantCultureIgnoreCase) &&
                //(
                //(Config.PluginBundles.ShowInstalled) ||
                //(false) // Config.Plugins.ShowAvailable
                //) &&
                (
                    (Config.PluginBundles.ShowOfKindPlatformSupport && plugin.PluginKinds.Contains(PluginKind.PlatformSupport)) ||
                    (Config.PluginBundles.ShowOfKindMetadata && plugin.PluginKinds.Contains(PluginKind.Metadata))
                )
        };
        Config.PluginBundles.PropertyChanged += OnConfigPropertyChanged;

        pluginManager.SubscribeLoadContext(
            loadContext => Manifests.Add(loadContext.Manifest),
            loadContext => Manifests.Remove(loadContext.Manifest));

        logger.LogInformation("[PluginBundlesViewModel-.ctor] PluginBundlesViewModel has been initialized");
    }



    void OnConfigPropertyChanged(object? _, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "Sorting":
                Manifests.KeySelector = Config.PluginBundles.Sorting switch
                {
                    Sorting.Default => null,
                    Sorting.Title => plugin => plugin.Name,
                    Sorting.Artist => plugin => plugin.Author,
                    Sorting.Duration => plugin => plugin.LastUpdatedAt,
                    _ => null
                };
                logger.LogInformation("[PluginBundlesViewModel-OnViewOptionsPropertyChanged] Reordered manifests");
                break;
            case "SortDescending":
                Manifests.Descending = Config.PluginBundles.SortDescending;
                logger.LogInformation("[PluginBundlesViewModel-OnViewOptionsPropertyChanged] Reordered manifests");
                break;
            case "ShowOfKindPlatformSupport":
            case "ShowOfKindMetadata":
            case "ShowInstalled":
            case "ShowAvailable":
                Manifests.Refresh();
                logger.LogInformation("[PluginBundlesViewModel-OnViewOptionsPropertyChanged] Refreshed manifests");
                break;
        }
    }


    public ObservableRangeCollection<Manifest> Manifests { get; }


    [ObservableProperty]
    Manifest? selectedManifest = null;

    async partial void OnSelectedManifestChanged(
        Manifest? value)
    {
        if (value is null)
            return;

        logger.LogInformation("[PluginBundlesViewModel-OnSelectedSearchResultChanged] Creating PluginBundleInfoViewModel...");

        PluginBundleInfoViewModel viewModel = App.Provider.GetRequiredService<PluginBundleInfoViewModel>();
        viewModel.Manifest = value;

        if (await mainView.AlertAsync(new PluginBundleInfoView(viewModel), $"Plugin: {value.Name}", "Close", "Remove") != ContentDialogResult.Primary)
        {
            SelectedManifest = null;
            logger.LogInformation("[PluginBundlesViewModel-OnSelectedManifestChanged] Plugin bundle info was shown: {name}", value.Name);
            return;
        }

        if (await mainView.AlertAsync("By removing this plugin bundle you will also remove all its plugin configurations.", "Are you sure?", "No", "Yes") != ContentDialogResult.Primary)
        {
            SelectedManifest = null;
            return;
        }

        try
        {
            PluginLoadContext loadContext = PluginManager.LoadedContexts.Distinct().FirstOrDefault(loadContext => loadContext.Manifest == value) ?? throw new("Could not find any plugin load context which matches the requested manifest.");

            PluginManager.Unload(loadContext);
            File.Delete(loadContext.Location);

            SelectedManifest = null;

            mainView.ShowNotification("Success!", $"Removed plugin bundle: {value.Name}.", NotificationLevel.Success);
            logger.LogInformation("[PluginBundlesViewModel-OnSelectedManifestChanged] Removed plugin bundle: {name}", value.Name);
        }
        catch (Exception ex)
        {
            SelectedManifest = null;

            mainView.ShowNotification("Something went wrong!", $"Failed to remove plugin bundle: {value.Name}.", NotificationLevel.Error, ex.ToFormattedString());
            logger.LogError(ex, "[PluginBundlesViewModel-OnSelectedManifestChanged] Failed to remove plugin bundle: {name}", value.Name);
        }
    }


    [ObservableProperty]
    string query = string.Empty;

    partial void OnQueryChanged(
        string value)
    {
        Manifests.Refresh();
        logger.LogInformation("[PluginBundlesViewModel-OnQueryChanged] Refreshed manifests");
    }


    public async Task<bool> TryLoadAsync(
        string path)
    {
        string bundleFileName = Path.GetFileNameWithoutExtension(path);
        try
        {
            await PluginManager.LoadBundleAsync(path);
            Config.Downloads.SelectedMetadatePlugin = PluginManager.GetLoadedOrDefault<MetadataPlugin>(Config.Downloads.SelectedMetadatePlugin)?.Name;

            mainView.ShowNotification("Success!", $"Loaded plugin bundle: {bundleFileName}.", NotificationLevel.Success);
            return true;
        }
        catch (PluginNotLoadedException ex) when (
            ex.InnerException is not null &&
            ex.InnerException.InnerException is PluginConfigException &&
            ex.PluginType is not null)
        {
            mainView.ShowNotification("Warning!", $"Could not load plugin: {ex.PluginType.Name}.", NotificationLevel.Warning, async () =>
            {
                if (await mainView.AlertAsync(
                    new()
                    {
                        Content = ex.ToFormattedString("Resetting the config may be able to fix this isuee.\nDo you want to reset the config for this plugin bundle and restart the app?"),
                        Title = "Warning!",
                        CloseButtonText = "No",
                        PrimaryButtonText = "Yes"
                    }) != ContentDialogResult.Primary)
                    return;

                Config.PluginBundles.Configs.Remove(ex.PluginType.Name);
                logger.LogInformation(ex, "[AppStartupHandler-TryLoadAsync] Reset plugins config: {pluginType}. Restarting now...", ex.PluginType.Name);

                Application.Current.Exit();
                AppInstance.Restart(null);
            });
            logger.LogWarning(ex, "[AppStartupHandler-TryLoadAsync] Could not load plugin: {name}: {exception}", ex.PluginType.Name, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            mainView.ShowNotification("Warning!", $"Could not load plugin bundle: {bundleFileName}.", NotificationLevel.Warning, ex.ToFormattedString());
            logger.LogWarning(ex, "[AppStartupHandler-TryLoadAsync] Could not load plugin bundle: {bundleFileName}: {exception}", bundleFileName, ex.Message);
            return false;
        }
    }

    [RelayCommand]
    async Task ImportAsync()
    {
        FileOpenPicker picker = new()
        {
            CommitButtonText = "Import plugin bundle",
            SuggestedStartLocation = PickerLocationId.Downloads,
            SettingsIdentifier = "Import plugin bundle",
        };
        picker.FileTypeFilter.Add(".mlr");
        mainView.Initialize(picker);

        StorageFile? file = await picker.PickSingleFileAsync();
        logger.LogInformation("[PluginBundlesViewModel-ImportAsync] File open picker was shown");

        if (file is null)
            return;

        string destination = Path.Combine(PluginManager.PluginsDirectory, file.Name);
        if (File.Exists(destination))
        {
            mainView.ShowNotification("Something went wrong!", "Failed to import plugin bundle.", NotificationLevel.Error, "It looks like a plugin bundle with the same name is already imported. Please first uninstall that plugin bundle.");
            logger.LogError(new Exception("It looks like a plugin bundle with the same name is already imported. Please first uninstall that plugin bundle."), "Failed to import plugin bundle: {fileName}", file.Name);
            return;
        }

        File.Copy(file.Path, destination);
        if (!await TryLoadAsync(destination))
        {
            try
            {
                await Task.Delay(1000);

                File.Delete(destination);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[PluginBundlesViewModel-ImportAsync] Failed to delete file: {exception}", ex.Message);
            }
            return;
        }

        logger.LogInformation("[PluginBundlesViewModel-ImportAsync] Plugin bundle was imported from file");
    }
}