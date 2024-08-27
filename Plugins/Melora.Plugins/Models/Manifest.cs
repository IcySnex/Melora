using Melora.Plugins.Enums;
using System.IO.Compression;
using System.Text.Json;

namespace Melora.Plugins.Models;

/// <summary>
/// Contains information for a plugin.
/// </summary>
/// <remarks>
/// Creates a new Manifest.
/// </remarks>
/// <param name="name">The name of the plugin.</param>
/// <param name="description">The description of the plugin.</param>
/// <param name="author">The author of the plugin.</param>
/// <param name="apiVersion">The version of the plugins API used.</param>
/// <param name="lastUpdatedAt">The date and time when the plugin was last updated..</param>
/// <param name="sourceUrl">The url to the source of the plugin (e.g. GitHub project, Website...).</param>
/// <param name="pluginKinds">The kinds of plugins contained.</param>
/// <param name="entryPoint">The path to the entry point of the plugin.</param>
/// <param name="dependencies">The paths to the dependencies of the plugin.</param>
public class Manifest(
    string name,
    string description,
    string author,
    Version apiVersion,
    DateTime lastUpdatedAt,
    string sourceUrl,
    PluginKind[] pluginKinds,
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
    /// The version of the plugins API used.
    /// </summary>
    public Version ApiVersion { get; } = apiVersion.Revision == -1 ? apiVersion : new(apiVersion.Major, apiVersion.Minor, apiVersion.Build);

    /// <summary>
    /// The date and time when the plugin was last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; } = lastUpdatedAt;

    /// <summary>
    /// The url to the source of the plugin (e.g. GitHub project, Website...).
    /// </summary>
    public string SourceUrl { get; } = sourceUrl;

    /// <summary>
    /// The kinds of plugins contained.
    /// </summary>
    public PluginKind[] PluginKinds { get; } = pluginKinds;

    /// <summary>
    /// The path to the entry point of the plugin.
    /// </summary>
    public string EntryPoint { get; } = entryPoint;

    /// <summary>
    /// The paths to the dependencies of the plugin.
    /// </summary>
    public string[] Dependencies { get; } = dependencies;
}