using Musify.Plugins.Enums;
using System.IO.Compression;
using System.Text.Json;

namespace Musify.Plugins;

/// <summary>
/// Contains information for a plugin.
/// </summary>
/// <param name="name">The name of the plugin.</param>
/// <param name="description">The description of the plugin.</param>
/// <param name="kind">The kind of the plugin.</param>
/// <param name="author">The author of the plugin.</param>
/// <param name="version">The version of the plugin.</param>
/// <param name="entryPoint">The path to the entry point of the plugin.</param>
/// <param name="dependencies">The paths to the dependencies of the plugin.</param>
public class Manifest(
    string name,
    string description,
    PluginKind kind,
    string author,
    Version version,
    string entryPoint,
    string[] dependencies)
{
    /// <summary>
    /// Gets the manifest from a plugin archive.
    /// </summary>
    /// <param name="pluginArchive">The plugin archive to load the manifest from.</param>
    /// <param name="cancellationToken">The token to cancel this action.</param>
    /// <returns>The manifest corresponding to the plugin.</returns>
    public static async Task<Manifest?> FromPluginArchivetAsync(
        ZipArchive pluginArchive,
        CancellationToken cancellationToken = default)
    {
        ZipArchiveEntry? file = pluginArchive.GetEntry("Manifest.json");
        if (file is null)
            return null;

        using Stream stream = file.Open();

        Manifest? manifest = await JsonSerializer.DeserializeAsync<Manifest>(stream, cancellationToken: cancellationToken);
        return manifest;
    }


    /// <summary>
    /// The name of the plugin.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The description of the plugin.
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// The kind of the plugin.
    /// </summary>
    public PluginKind Kind { get; } = kind;

    /// <summary>
    /// The author of the plugin.
    /// </summary>
    public string Author { get; } = author;

    /// <summary>
    /// The version of the plugin
    /// </summary>
    public Version Version { get; } = version;

    /// <summary>
    /// The path to the entry point of the plugin.
    /// </summary>
    public string EntryPoint { get; } = entryPoint;

    /// <summary>
    /// The paths to the dependencies of the plugin.
    /// </summary>
    public string[] Dependencies { get; } = dependencies;
}