using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Musify.Models;
using Musify.Plugins;
using Musify.Plugins.Abstract;
using Musify.Plugins.Exceptions;
using Musify.Plugins.Models;
using Serilog.Context;
using System.Reflection;

namespace Musify.Services;

public class PluginManager
{
    public static readonly string PluginsDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins");

    readonly ILogger<PluginManager> logger;
    readonly Config config;

    public PluginManager(
        ILogger<PluginManager> logger,
        Config config)
    {
        this.logger = logger;
        this.config = config;

        if (!Directory.Exists(PluginsDirectory))
            Directory.CreateDirectory(PluginsDirectory);

        logger.LogInformation("[PluginManager-.ctor] PluginManager has been initialized");
    }


    readonly Dictionary<IPlugin, PluginLoadContext> loadedPluginsLoadContexts = [];

    public IReadOnlyCollection<IPlugin> LoadedPlugins => loadedPluginsLoadContexts.Keys;

    public IReadOnlyCollection<PluginLoadContext> LoadedContexts => loadedPluginsLoadContexts.Values;


    public bool TryGetLoaded<T>(
        string? name,
        out T result) where T : IPlugin
    {
        result = default!;

        if (name is null)
            return false;

        IPlugin? plugin = LoadedPlugins.FirstOrDefault(loadedPlugin => loadedPlugin.Name == name);
        if (plugin is not T typedPlugin)
            return false;

        result = typedPlugin;
        return true;
    }

    public bool TryGetLoaded<T>(
        int? hash,
        out T result) where T : IPlugin
    {
        result = default!;

        if (!hash.HasValue)
            return false;

        IPlugin? plugin = LoadedPlugins.FirstOrDefault(loadedPlugin => loadedPlugin.GetHashCode() == hash.Value);
        if (plugin is not T typedPlugin)
            return false;

        result = typedPlugin;
        return true;
    }

    public T GetLoaded<T>(
        string? name) where T : IPlugin =>
        TryGetLoaded(name, out T result) ? result : throw new Exception($"Could not get plugin with specified name ({name}) and requested type.");
    
    public T GetLoaded<T>(
        int? hash) where T : IPlugin =>
        TryGetLoaded(hash, out T result) ? result : throw new Exception($"Could not get plugin with given hash ({hash}) and requested type.");

    public T? GetLoadedOrDefault<T>(
        string? name) where T : IPlugin
    {
        if (TryGetLoaded(name, out T result))
            return result;

        return LoadedPlugins.OfType<T>().FirstOrDefault();
    }

    public T? GetLoadedOrDefault<T>(
        int? hash) where T : IPlugin
    {
        if (TryGetLoaded(hash, out T result))
            return result;

        return LoadedPlugins.OfType<T>().FirstOrDefault();
    }


    readonly Dictionary<Type, HashSet<(Delegate? onLoad, Delegate? onUnload)>> subscriptions = [];

    public void Subscribe<T>(
        Action<T>? onLoad,
        Action<T>? onUnload) where T : IPlugin
    {
        Type pluginType = typeof(T);
        if (!subscriptions.TryGetValue(pluginType, out HashSet<(Delegate? onLoad, Delegate? onUnload)>? handlers))
            subscriptions[pluginType] = [(onLoad, onUnload)];
        else
            handlers.Add((onLoad, onUnload));

        logger.LogInformation("[PluginManager-Subscribe] Subscribed to plugin loading");
    }

    void OnPluginLoaded(
        Type pluginType,
        IPlugin plugin)
    {
        if (!subscriptions.TryGetValue(pluginType, out HashSet<(Delegate? onLoad, Delegate? onUnload)>? handlers))
            return;

        foreach ((Delegate? onLoad, _) in handlers)
            onLoad?.DynamicInvoke(plugin);
    }

    void OnPluginUnloaded(
        Type pluginType,
        IPlugin plugin)
    {
        if (!subscriptions.TryGetValue(pluginType, out HashSet<(Delegate? onLoad, Delegate? onUnload)>? handlers))
            return;

        foreach ((_, Delegate? onUnload) in handlers)
            onUnload?.DynamicInvoke(plugin);
    }


    readonly HashSet<(Action<PluginLoadContext>? onLoad, Action<PluginLoadContext>? onUnload)> loadContextSubscriptions = [];

    public void SubscribeLoadContext(
        Action<PluginLoadContext>? onLoad,
        Action<PluginLoadContext>? onUnload)
    {
        loadContextSubscriptions.Add((onLoad, onUnload));

        logger.LogInformation("[PluginManager-SubscribeLoadContext] Subscribed to plugin load context loading");
    }

    void OnLoadContextLoaded(
        PluginLoadContext context)
    {
        foreach ((Action<PluginLoadContext>? onLoad, _) in loadContextSubscriptions)
            onLoad?.Invoke(context);
    }

    void OnLoadContextUnloaded(
        PluginLoadContext context)
    {
        foreach ((_, Action<PluginLoadContext>? onUnload) in loadContextSubscriptions)
            onUnload?.Invoke(context);
    }



    public async Task LoadBundleAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[PluginManager-LoadAsync] Starting to load plugin bundle: [{path}]...", path);
        PluginLoadContext loadContext = await PluginLoadContext.FromPluginArchiveAsync(path, cancellationToken);

        foreach (Type type in loadContext.EntryPointAssembly.GetExportedTypes())
        {
            if (!typeof(IPlugin).IsAssignableFrom(type))
                continue;

            Type configType = type switch
            {
                var t when typeof(PlatformSupportPlugin).IsAssignableFrom(t) => typeof(PlatformSupportPluginConfig),
                var t when typeof(MetadataPlugin).IsAssignableFrom(t) => typeof(MetadataPluginConfig),
                _ => typeof(IPluginConfig)
            };
            bool isConfigSaved = config.Plugins.Configs.TryGetValue(type.Name, out IPluginConfig? pluginConfig) && pluginConfig.GetType() == configType;

            ConstructorInfo? constructor = null;
            object?[]? constructorArgs = null;

            if (isConfigSaved && type.GetConstructor([configType, typeof(ILogger<IPlugin>)]) is ConstructorInfo configLoggerContructor)
            {
                constructor = configLoggerContructor;
                constructorArgs = [pluginConfig, App.Provider.GetRequiredService<ILogger<IPlugin>>()];
            }
            else if (isConfigSaved && type.GetConstructor([configType]) is ConstructorInfo configContructor)
            {
                constructor = configContructor;
                constructorArgs = [pluginConfig];
            }
            else if (type.GetConstructor([typeof(ILogger<IPlugin>)]) is ConstructorInfo loggerContructor)
            {
                constructor = loggerContructor;
                constructorArgs = [App.Provider.GetRequiredService<ILogger<IPlugin>>()];
            }
            else if (type.GetConstructor(Type.EmptyTypes) is ConstructorInfo defaultConstructor)
            {
                constructor = defaultConstructor;
            }

            try
            {
                IPlugin plugin = (IPlugin?)constructor?.Invoke(constructorArgs) ?? throw new Exception("Could not find suitable constructors to create plugin instance.");

                if (!LoadedContexts.Contains(loadContext))
                    OnLoadContextLoaded(loadContext);

                loadedPluginsLoadContexts[plugin] = loadContext;
                OnPluginLoaded(type.BaseType!, plugin);

                logger.LogInformation("[PluginManager-LoadAsync] Loaded plugin: [{name}]", plugin.Name);
            }
            catch (Exception ex)
            {
                throw new PluginNotLoadedException(path, type, loadContext.Manifest, ex);
            }
        }
    }


    public void Unload(
        IPlugin plugin)
    {
        logger.LogInformation("[PluginManager-Unload] Starting to unload plugin: [{name}]...", plugin.Name);
        bool result = loadedPluginsLoadContexts.TryGetValue(plugin, out PluginLoadContext? loadContext);
        if (!result)
            throw new Exception("Could not find plugin. Are you sure the plugin was loaded?");

        loadedPluginsLoadContexts.Remove(plugin);

        Type pluginType = plugin.GetType().BaseType!;
        OnPluginUnloaded(pluginType, plugin);

        if (subscriptions.TryGetValue(pluginType, out HashSet<(Delegate?, Delegate?)>? handlers) && handlers.Count == 0)
            subscriptions.Remove(pluginType);

        if (!LoadedContexts.Contains(loadContext))
        {
            loadContext!.Unload();
            OnLoadContextUnloaded(loadContext);
        }

        logger.LogInformation("[PluginManager-Unload] Unloaded plugin: [{name}]", plugin.Name);
    }

    public void Unload(
        PluginLoadContext loadContext)
    {
        logger.LogInformation("[PluginManager-Unload] Starting to unload plugin load context: [{manifestName}]...", loadContext.Manifest.Name);
        IEnumerable<IPlugin> plugins = loadedPluginsLoadContexts.Where(pair => pair.Value == loadContext).Select(pair => pair.Key);

        foreach (IPlugin plugin in plugins)
        {
            loadedPluginsLoadContexts.Remove(plugin);

            Type pluginType = plugin.GetType().BaseType!;
            OnPluginUnloaded(pluginType, plugin);

            if (subscriptions.TryGetValue(pluginType, out HashSet<(Delegate?, Delegate?)>? handlers) && handlers.Count == 0)
                subscriptions.Remove(pluginType);
        }

        loadContext!.Unload();
        OnLoadContextUnloaded(loadContext);

        logger.LogInformation("[PluginManager-Unload] Unloaded plugin load context: [{manifestName}]", loadContext.Manifest.Name);
    }
}