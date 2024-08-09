using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Musify.Plugins.Models;
using Musify.Views;

namespace Musify.ViewModels;

public partial class PluginInfoViewModel : ObservableObject
{
    readonly ILogger<PluginInfoViewModel> logger;
    readonly MainView mainView;

    public PluginInfoViewModel(
        ILogger<PluginInfoViewModel> logger,
        MainView mainView)
    {
        this.logger = logger;
        this.mainView = mainView;

        logger.LogInformation("[PluginInfoViewModel-.ctor] PluginInfoViewModel has been initialized");
    }


    public Manifest PluginBundle { get; set; } = default!;

}