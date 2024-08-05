using CommunityToolkit.Mvvm.ComponentModel;

namespace Musify.Plugins.Models;

/// <summary>
/// Represnts a single item of a plugin config.
/// </summary>
/// <remarks>
/// Creates a new PluginConfigItem
/// </remarks>
/// <param name="name">The name of the property.</param>
/// <param name="description">The description of the property.</param>
/// <param name="value">The value of the property.</param>
public partial class PluginConfigItem(
    string name,
    string description,
    object value) : ObservableObject, ICloneable
{
    /// <summary>
    /// The name of the property.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The description of the property.
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// The value of the property.
    /// </summary>
    [ObservableProperty]
    object value = value;


    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    public object Clone() =>
        new PluginConfigItem(Name, Description, Value);
}