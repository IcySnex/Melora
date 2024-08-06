using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using Musify.Enums;
using Musify.Helpers;
using Musify.Models;
using Musify.Plugins.Abstract;
using Musify.Plugins.Exceptions;
using Musify.Views;

namespace Musify.Services;

public class AppStartupHandler
{
    public AppStartupHandler(
        ILogger<AppStartupHandler> logger,
        Config config,
        PluginManager pluginManager,
        MainView mainView,
        Navigation navigation)
    {
        mainView.SetSize(1100, 560);
        mainView.SetMinSize(700, 525);
        mainView.SetIcon("icon.ico");
        mainView.Activate();

        navigation.SetCurrentItem("Home");

        async void LoadPlugins()
        {
            foreach (string path in Directory.GetFiles(PluginManager.PluginsDirectory, "*.mfy"))
            {
                string pluginFileName = Path.GetFileNameWithoutExtension(path);
                try
                {
                    await pluginManager.LoadPluginAsync(path);
                    mainView.ShowNotification("Success!", $"Loaded plugin: {pluginFileName}.", NotificationLevel.Success);
                }
                catch (PluginNotLoadedException ex)
                {
                    mainView.ShowNotification("Warning!", $"Failed to load plugin: {pluginFileName}.", NotificationLevel.Warning, async () =>
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

                        config.PluginConfigs.Remove(ex.PluginType!.Name);

                        mainView.Close();
                        AppInstance.Restart(null);
                    });
                    logger.LogWarning(ex, "[AppStartupHandler-LoadPlugins] Failed to load plugin: {pluginFileName}: {exception}", pluginFileName, ex.Message);
                }
                catch (Exception ex)
                {
                    mainView.ShowNotification("Warning!", $"Failed to load plugin: {pluginFileName}.", NotificationLevel.Warning, ex.ToFormattedString());
                    logger.LogWarning(ex, "[AppStartupHandler-LoadPlugins] Failed to load plugin: {pluginFileName}: {exception}", pluginFileName, ex.Message);
                }
            }

            config.Downloads.SelectedMetadatePlugin = pluginManager.GetLoadedOrDefault<MetadataPlugin>(config.Downloads.SelectedMetadatePlugin)?.Name;
        };
        LoadPlugins();

        logger.LogInformation("[AppStartupHandler-.ctor] AppStartupHandler has been initialized");
    }
}