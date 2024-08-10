using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Musify.Plugins.Models;

namespace Musify.ViewModels;

public partial class PluginBundleInfoViewModel : ObservableObject
{
    public PluginBundleInfoViewModel(
        ILogger<PluginBundleInfoViewModel> logger)
    {
        logger.LogInformation("[PluginBundleInfoViewModel-.ctor] PluginBundleInfoViewModel has been initialized");
    }


    public Manifest Manifest { get; set; } = default!;
}