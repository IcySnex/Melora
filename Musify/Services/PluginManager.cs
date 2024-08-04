using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Musify.Models;
using Musify.Plugins;
using Musify.Plugins.Abstract;
using Musify.Plugins.Exceptions;
using Musify.Plugins.Models;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Musify.Services;

public class PluginManager<T> where T : IPlugin
{
    public static readonly string PluginsDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins");


    readonly ILogger<PluginManager<T>> logger;
    readonly Config config;

    public PluginManager(
        ILogger<PluginManager<T>> logger,
        Config config)
    {
        this.logger = logger;
        this.config = config;

        if (!Directory.Exists(PluginsDirectory))
            Directory.CreateDirectory(PluginsDirectory);

        logger.LogInformation("[PluginManager-.ctor] PluginManager has been initialized");
    }


    public ObservableCollection<T> LoadedPlugins { get; } = [];

    public async Task LoadPluginAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[PluginManager-LoadPluginAsync] Starting loading plugin: [{path}]...", path);
        PluginLoadContext loadContext = await PluginLoadContext.FromPluginArchiveAsync(path, cancellationToken);

        foreach (Type type in loadContext.EntryPointAssembly.GetExportedTypes())
        {
            if (!typeof(T).IsAssignableFrom(type))
                continue;

            Type configType = type switch
            {
                _ when typeof(PlatformSupportPlugin).IsAssignableFrom(type) => typeof(PlatformSupportPluginConfig),
                _ when typeof(MetadataPlugin).IsAssignableFrom(type) => typeof(MetadataPluginConfig),
                _ => typeof(IPluginConfig)
            };
            bool isConfigSaved = config.PluginConfigs.TryGetValue(type.Name, out IPluginConfig? pluginConfig) && pluginConfig.GetType() == configType;

            ConstructorInfo? constructor = null;
            object?[]? constructorArgs = null;

            if (isConfigSaved && type.GetConstructor([configType, typeof(ILogger<T>)]) is ConstructorInfo configLoggerContructor)
            {
                constructor = configLoggerContructor;
                constructorArgs = [pluginConfig, App.Provider.GetRequiredService<ILogger<T>>()];
            }
            else if (isConfigSaved && type.GetConstructor([configType]) is ConstructorInfo configContructor)
            {
                constructor = configContructor;
                constructorArgs = [pluginConfig];
            }
            else if (type.GetConstructor([typeof(ILogger<T>)]) is ConstructorInfo loggerContructor)
            {
                constructor = loggerContructor;
                constructorArgs = [App.Provider.GetRequiredService<ILogger<T>>()];
            }
            else if (type.GetConstructor(Type.EmptyTypes) is ConstructorInfo defaultConstructor)
            {
                constructor = defaultConstructor;
            }

            try
            {
                T? plugin = (T?)constructor?.Invoke(constructorArgs) ?? throw new Exception("Could not find suitable constructors to create plugin instance.");

                logger.LogInformation("[PluginManager-LoadPluginAsync] Loaded plugin: [{name}]...", plugin.Name);
                LoadedPlugins.Add(plugin);
            }
            catch (Exception ex)
            {
                throw new PluginNotLoadedException(path, type, loadContext.Manifest, ex);
            }
        }
    }
}