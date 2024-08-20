using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Melora.Plugins.Models;
using Microsoft.Extensions.Logging;
using Windows.System;

namespace Melora.ViewModels;

public partial class PluginBundleInfoViewModel : ObservableObject
{
    readonly ILogger<PluginBundleInfoViewModel> logger;

    public PluginBundleInfoViewModel(
        ILogger<PluginBundleInfoViewModel> logger)
    {
        this.logger = logger;

        logger.LogInformation("[PluginBundleInfoViewModel-.ctor] PluginBundleInfoViewModel has been initialized");
    }


    public Manifest Manifest { get; set; } = default!;


    [RelayCommand]
    async Task OpenSourceAsync()
    {
        await Launcher.LaunchUriAsync(new(Manifest.SourceUrl));
        logger.LogInformation("[DownloadsViewModel-OpenSourceAsync] Browser was opened with plugin source url");
    }
}