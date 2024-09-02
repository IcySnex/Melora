using CommunityToolkit.Mvvm.ComponentModel;
using Melora.Plugins.Abstract;
using Melora.Plugins.Exceptions;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Melora.Plugins.Models;

/// <summary>
/// Describes a configuration for a metadata plugin
/// </summary>
public partial class MetadataPluginConfig : ObservableObject, IPluginConfig
{
    /// <summary>
    /// Creates a new MetadataPluginConfig
    /// </summary>
    /// <param name="options">Additional config options for the plugin.</param>
    [JsonConstructor]
    public MetadataPluginConfig(
        IOption[] options) : this(options, null)
    { }

    /// <summary>
    /// Creates a new MetadataPluginConfig
    /// </summary>
    /// <param name="defaultOptions">Default additional config options for the plugin.</param>
    /// <param name="initialConfig">The config used for initializing if exists.</param>
    public MetadataPluginConfig(
        IOption[] defaultOptions,
        MetadataPluginConfig? initialConfig = null)
    {
        // Set default values
        this.defaultOptions = defaultOptions;

        // Inital config config passed: Set to default values
        if (initialConfig is null)
        {
            Options = [.. defaultOptions];
            foreach (IOption option in Options)
                option.PropertyChanged += OnItemPropertyChanged;

            Reset();
            return;
        }

        // Inital config passed: Validate and set to inital config values
        if (!initialConfig.ContainsAll(defaultOptions))
            throw new PluginConfigException(this, new("Passed initial config does not match additional items requiered for the plugin."));

        Options = initialConfig.Options.Select((option, i) => defaultOptions[i].Copy(option.Value)).ToArray();
        foreach (IOption option in Options)
            option.PropertyChanged += OnItemPropertyChanged;
    }


    void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        OnPropertyChanged(((IOption)sender!).Name);


    /// <summary>
    /// Additional config options for the plugin.
    /// </summary>
    public IOption[] Options { get; } = default!;
    readonly IOption[] defaultOptions;


    /// <summary>
    /// Resets the config to the plugins default.
    /// </summary>
    public void Reset()
    {
        for (int i = 0; i < Options.Length; i++)
            Options[i].Value = defaultOptions[i].Value;
    }
}