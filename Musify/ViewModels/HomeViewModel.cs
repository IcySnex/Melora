using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Musify.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    readonly ILogger<HomeViewModel> logger;

    public HomeViewModel(
        ILogger<HomeViewModel> logger)
    {
        this.logger = logger;

        logger.LogInformation("[HomeViewModel-.ctor] HomeViewModel has been initialized");
    }
}