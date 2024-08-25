using Melora.Models;
using Melora.ViewModels;
using Melora.Views;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace Melora.Services;

public class AppStartupHandler
{
    public AppStartupHandler(
        ILogger<AppStartupHandler> logger,
        Config config,
        MainView mainView,
        Navigation navigation,
        PluginBundlesViewModel pluginBundlesViewModel,
        SettingsViewModel settingsViewModel)
    {
        ((FrameworkElement)mainView.Content).Loaded += async (s, e) =>
        {
            foreach (string path in Directory.GetFiles(PluginManager.PluginsDirectory, "*.mlr"))
                await pluginBundlesViewModel.TryLoadAsync(path);

            if (config.Updates.AutomaticUpdateCheck)
                await settingsViewModel.TryGetUpdatesAsync();
        };

        mainView.SetSize(1100, 559);
        mainView.SetMinSize(700, 525);
        mainView.SetIcon("icon.ico");
        mainView.Activate();

        navigation.SetCurrentItem("Home");

        logger.LogInformation("[AppStartupHandler-.ctor] AppStartupHandler has been initialized");
    }
}