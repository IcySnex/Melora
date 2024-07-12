using Microsoft.Extensions.Logging;
using Musify.Enums;
using Musify.Plugins;
using Musify.Plugins.Abstract;
using Musify.Plugins.Exceptions;
using Musify.Views;

namespace Musify.Services;

public class PluginManager<T> where T : IPlugin
{
    public static readonly string PluginsDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins");


    readonly ILogger<PluginManager<IPlugin>> logger;
    readonly MainView mainView;

    public PluginManager(
        ILogger<PluginManager<IPlugin>> logger,
        MainView mainView)
    {
        this.logger = logger;
        this.mainView = mainView;

        if (!Directory.Exists(PluginsDirectory))
            Directory.CreateDirectory(PluginsDirectory);

        logger.LogInformation("[PluginManager-.ctor] PluginManager has been initialized");
    }


    public List<IPlugin> LoadedPlugins { get; } = [];


    public async Task LoadPluginAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[PluginManager-LoadPluginAsync] Starting loading plugin: [{path}]...", path);
        PluginLoadContext loadContext = await PluginLoadContext.FromPluginArchiveAsync(path, cancellationToken);

        int loadedPluginsCount = LoadedPlugins.Count;
        foreach (Type type in loadContext.EntryPointAssembly.GetTypes())
        {
            if (!typeof(IPlugin).IsAssignableFrom(type) || type.GetConstructor(Type.EmptyTypes) is null)
                continue;

            IPlugin? plugin = (IPlugin?)Activator.CreateInstance(type);
            if (plugin is null)
                continue;

            logger.LogInformation("[PluginManager-LoadPluginAsync] Loaded plugin: [{name}]...", plugin.Name);
            LoadedPlugins.Add(plugin);
        }

        if (loadedPluginsCount == LoadedPlugins.Count)
            throw new PluginNotLoadedException(path, null, new("Could not find any suitable types."));
    }

    public async Task LoadAllPluginsAsync(
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[PluginManager-LoadAllPluginsAsync] Starting loading all plugins...");

        string pluginFileName = "";
        try
        {
            foreach (string path in Directory.GetFiles(PluginsDirectory, "*.mfy"))
            {
                pluginFileName = Path.GetFileNameWithoutExtension(path);
                await LoadPluginAsync(path, cancellationToken);

                mainView.ShowNotification("Success!", $"Loaded plugin: {pluginFileName}.", NotificationLevel.Success);
            }
        }
        catch (Exception ex)
        {
            mainView.ShowNotification("Something went wrong!", $"Failed to load plugin: {pluginFileName}.", NotificationLevel.Error, $"{ex.Message}\n{ex.InnerException?.Message}");
            logger.LogError("[PluginManager-LoadAllPluginsAsync] Failed to load plugin: {pluginFileName}: {exception}", pluginFileName, ex.Message);
        }
    }
}