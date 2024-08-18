﻿using CommunityToolkit.Mvvm.ComponentModel;
using Melora.Plugins.Models;
using Microsoft.Extensions.Logging;

namespace Melora.ViewModels;

public partial class PluginBundleInfoViewModel : ObservableObject
{
    public PluginBundleInfoViewModel(
        ILogger<PluginBundleInfoViewModel> logger)
    {
        logger.LogInformation("[PluginBundleInfoViewModel-.ctor] PluginBundleInfoViewModel has been initialized");
    }


    public Manifest Manifest { get; set; } = default!;
}