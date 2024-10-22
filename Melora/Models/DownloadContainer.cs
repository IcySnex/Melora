﻿using CommunityToolkit.Mvvm.ComponentModel;
using Melora.Plugins.Models;

namespace Melora.Models;

public partial class DownloadContainer(
    DownloadableTrack track,
    int pluginHash) : ObservableObject, IDisposable
{
    bool isDisposed;

    public bool IsDisposed => isDisposed;

    protected virtual void Dispose(
        bool disposing)
    {
        if (isDisposed)
            return;

        if (disposing)
        {
            CancellationSource.Dispose();
        }
        isDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~DownloadContainer() =>
        Dispose(false);


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIdle))]
    int progress = -1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIdle))]
    bool isProcessing = false;

    public bool IsIdle => !IsProcessing && Progress == -1;

    public DownloadableTrack Track { get; } = track;

    public int PluginHash { get; } = pluginHash;

    public CancellationTokenSource CancellationSource { get; private set; } = new();


    public void Reset()
    {
        if (isDisposed)
            return;

        Progress = -1;
        IsProcessing = false;
        CancellationSource.Dispose();
        CancellationSource = new();
    }
}