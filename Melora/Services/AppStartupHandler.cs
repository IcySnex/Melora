using Melora.ViewModels;
using Melora.Views;
using Microsoft.Extensions.Logging;

namespace Melora.Services;

public class AppStartupHandler
{
    public AppStartupHandler(
        ILogger<AppStartupHandler> logger,
        MainView mainView,
        Navigation navigation,
        PluginBundlesViewModel pluginBundlesViewModel)
    {
        mainView.SetSize(1100, 559);
        mainView.SetMinSize(700, 525);
        mainView.SetIcon("icon.ico");
        mainView.Activate();

        navigation.SetCurrentItem("Home");

        mainView.DispatcherQueue.TryEnqueue(async () =>
        {
            foreach (string path in Directory.GetFiles(PluginManager.PluginsDirectory, "*.mlr"))
                await pluginBundlesViewModel.TryLoadAsync(path);
        });

        logger.LogInformation("[AppStartupHandler-.ctor] AppStartupHandler has been initialized");
    }
}