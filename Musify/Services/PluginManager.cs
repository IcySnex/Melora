using Microsoft.Extensions.Logging;
using Musify.Plugins;
using Musify.Plugins.Abstract;
using Musify.Plugins.Exceptions;

namespace Musify.Services;

public class PluginManager<T> where T : IPlugin
{
    public static readonly string PluginsDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins");


    readonly ILogger<PluginManager<IPlugin>> logger;

    public PluginManager(
        ILogger<PluginManager<IPlugin>> logger)
    {
        this.logger = logger;

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

        foreach (string path in Directory.GetFiles(PluginsDirectory, "*.mfy"))
            await LoadPluginAsync(path, cancellationToken);
    }

}