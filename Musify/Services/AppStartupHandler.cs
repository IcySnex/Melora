using Microsoft.Extensions.Logging;
using Musify.Plugins.Abstract;
using Musify.Views;

namespace Musify.Services;

public class AppStartupHandler
{
    readonly ILogger<AppStartupHandler> logger;
    readonly PluginManager<IPlugin> pluginManager;
    readonly MainView mainView;
    readonly Navigation navigation;

    public AppStartupHandler(
        ILogger<AppStartupHandler> logger,
        PluginManager<IPlugin> pluginManager,
        MainView mainView,
        Navigation navigation)
    {
        this.logger = logger;
        this.pluginManager = pluginManager;
        this.mainView = mainView;
        this.navigation = navigation;

        logger.LogInformation("[AppStartupHandler-.ctor] AppStartupHandler has been initialized");
    }

    public async Task PrepareStartupAsync()
    {
        navigation.SetCurrentIndex(0);
        navigation.Navigate("Home");

        mainView.SetSize(1100, 559);
        mainView.SetMinSize(670, 470);
        mainView.SetIcon("icon.ico");
        mainView.Activate();

        await pluginManager.LoadAllPluginsAsync();

        logger.LogInformation("[AppStartupHandler-PrepareStartupAsync] App fully started");
    }
}