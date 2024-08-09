using Microsoft.Extensions.Logging;
using Musify.Models;
using Musify.Plugins.Abstract;
using Musify.ViewModels;
using Musify.Views;

namespace Musify.Services;

public class AppStartupHandler
{
    public AppStartupHandler(
        ILogger<AppStartupHandler> logger,
        Config config,
        PluginManager pluginManager,
        MainView mainView,
        Navigation navigation,
        PluginsViewModel pluginsViewModel)
    {
        mainView.SetSize(1150, 567);
        mainView.SetMinSize(700, 525);
        mainView.SetIcon("icon.ico");
        mainView.Activate();

        navigation.SetCurrentItem("Home");

        mainView.DispatcherQueue.TryEnqueue(async () =>
        {
            foreach (string path in Directory.GetFiles(PluginManager.PluginsDirectory, "*.mfy"))
                await pluginsViewModel.TryLoadAsync(path);

            config.Downloads.SelectedMetadatePlugin = pluginManager.GetLoadedOrDefault<MetadataPlugin>(config.Downloads.SelectedMetadatePlugin)?.Name;
        });

        logger.LogInformation("[AppStartupHandler-.ctor] AppStartupHandler has been initialized");
    }
}