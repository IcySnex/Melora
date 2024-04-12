using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Musify.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    readonly ILogger<SettingsViewModel> logger;

    public SettingsViewModel(
        ILogger<SettingsViewModel> logger)
    {
        this.logger = logger;

        logger.LogInformation("[SettingsViewModel-.ctor] SettingsViewModel has been initialized");
    }
}