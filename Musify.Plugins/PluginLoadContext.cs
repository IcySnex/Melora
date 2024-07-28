using Musify.Plugins.Exceptions;
using Musify.Plugins.Models;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Loader;

namespace Musify.Plugins;

/// <summary>
/// Represents the runtime's concept of a scope for plugin loading.
/// </summary>
public class PluginLoadContext : AssemblyLoadContext
{
    /// <summary>
    /// The plugin manifest corresponding to this load context.
    /// </summary>
    public Manifest Manifest { get; }

    /// <summary>
    /// Creates a new instance of PluginLoadContext.
    /// </summary>
    /// <param name="manifest">The plugin manifest corresponding to this load context.</param>
    private PluginLoadContext(
        Manifest manifest)
    {
        this.Manifest = manifest;
    }


    /// <summary>
    /// The assembly of the plugin entry point.
    /// </summary>
    public Assembly EntryPointAssembly { get; internal set; } = default!;


    /// <summary>
    /// Creates a new instance of PluginLoadContext from a plugin archive.
    /// </summary>
    /// <param name="path">The path to the plugin archive.</param>
    /// <param name="cancellationToken">The token to cancel this action.</param>
    /// <returns>The PluginLoadContext for the plugin archive.</returns>
    /// <exception cref="PluginNotLoadedException">Occurrs when the plugin could not be loaded.</exception>
    public static async Task<PluginLoadContext> FromPluginArchiveAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        using ZipArchive pluginArchive = ZipFile.OpenRead(path);

        Manifest manifest = await Manifest.FromPluginArchivetAsync(pluginArchive, cancellationToken)?? throw new PluginNotLoadedException(path, null, null, new("Plugin archive does not contain a manifest or is badly formatted."));
        PluginLoadContext loadContext = new(manifest);

        loadContext.EntryPointAssembly = await loadContext.LoadFromArchiveAsync(pluginArchive, manifest.EntryPoint, cancellationToken) ?? throw new PluginNotLoadedException(path, null, manifest, new($"Entry point assembly could not be loaded: [{manifest.EntryPoint}]."));
        foreach (string dependency in manifest.Dependencies)
            if (await loadContext.LoadFromArchiveAsync(pluginArchive, dependency, cancellationToken) is null)
                throw new PluginNotLoadedException(path, null, manifest, new($"Dependency assembly could not be loaded: [{dependency}]."));

        return loadContext;
    }


    /// <summary>
    /// Loads the assembly with an archive.
    /// </summary>
    /// <param name="archive">The archive to load the assembly from.</param>
    /// <param name="path">The path to the assembly file inside the archive.</param>
    /// <param name="cancellationToken">The token to cancel this action.</param>
    /// <returns>The loaded assembly.</returns>
    public async Task<Assembly?> LoadFromArchiveAsync(
        ZipArchive archive,
        string path,
        CancellationToken cancellationToken = default)
    {
        ZipArchiveEntry? file = archive.GetEntry(path);
        if (file is null)
            return null;

        using Stream entryStream = file.Open();
        using MemoryStream ms = new();

        await entryStream.CopyToAsync(ms, cancellationToken);
        ms.Position = 0;

        return LoadFromStream(ms);
    }
}