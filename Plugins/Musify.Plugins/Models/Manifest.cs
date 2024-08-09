using Musify.Plugins.Enums;
using System.IO.Compression;
using System.Text.Json;

namespace Musify.Plugins.Models;

/// <summary>
/// Contains information for a plugin.
/// </summary>
/// <remarks>
/// Creates a new Manifest.
/// </remarks>
/// <param name="name">The name of the plugin.</param>
/// <param name="description">The description of the plugin.</param>
/// <param name="author">The author of the plugin.</param>
/// <param name="pluginKinds">The kinds of plugins contained.</param>
/// <param name="lastUpdatedAt">The date and time when the plugin was last updated..</param>
/// <param name="entryPoint">The path to the entry point of the plugin.</param>
/// <param name="dependencies">The paths to the dependencies of the plugin.</param>
public class Manifest(
    string name,
    string description,
    string author,
    PluginKind[] pluginKinds,
    DateTime lastUpdatedAt,
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
    /// The author of the plugin.
    /// </summary>
    public string Author { get; } = author;

    /// <summary>
    /// The kinds of plugins contained.
    /// </summary>
    public PluginKind[] PluginKinds { get; } = pluginKinds;

    /// <summary>
    /// The date and time when the plugin was last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; } = lastUpdatedAt;

    /// <summary>
    /// The path to the entry point of the plugin.
    /// </summary>
    public string EntryPoint { get; } = entryPoint;

    /// <summary>
    /// The paths to the dependencies of the plugin.
    /// </summary>
    public string[] Dependencies { get; } = dependencies;
}