using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Musify.Plugins.Models;
using Musify.Views;

namespace Musify.ViewModels;

public partial class PluginBundleInfoViewModel : ObservableObject
{
    readonly ILogger<PluginBundleInfoViewModel> logger;
    readonly MainView mainView;

    public PluginBundleInfoViewModel(
        ILogger<PluginBundleInfoViewModel> logger,
        MainView mainView)
    {
        this.logger = logger;
        this.mainView = mainView;

        logger.LogInformation("[PluginBundleInfoViewModel-.ctor] PluginBundleInfoViewModel has been initialized");
    }


    public Manifest Manifest { get; set; } = default!;
}