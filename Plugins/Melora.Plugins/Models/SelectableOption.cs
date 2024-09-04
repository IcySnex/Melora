using CommunityToolkit.Mvvm.ComponentModel;
using Melora.Plugins.Abstract;
using System.Text.Json.Serialization;

namespace Melora.Plugins.Models;

/// <summary>
/// Represents a custom selectable option for a plugin config.
/// </summary>
/// <remarks>
/// Creates a new SelectableOption.
/// </remarks>
/// <param name="name">The name of the option.</param>
/// <param name="description">The description of the option.</param>
/// <param name="value">The value of the option.</param>
/// <param name="items">The items from which can be selected.</param>
public partial class SelectableOption(
    string name,
    string description,
    string value,
    string[] items) : ObservableObject, IOption
{
    /// <summary>
    /// The name of the option.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The description of the option.
    /// </summary>
    [JsonIgnore]
    public string Description { get; } = description;

    /// <summary>
    /// The value of the option.
    /// </summary>
    [ObservableProperty]
    string value = value;
    object IOption.Value
    {
        get => Value;
        set => Value = (string)value;
    }

    /// <summary>
    /// The items from which can be selected.
    /// </summary>
    [JsonIgnore]
    public string[] Items { get; } = items;


    /// <summary>
    /// Creates a new object that is a copy of the current instance with the new value.
    /// </summary>
    /// <param name="value">The new value of the copy.</param>
    /// <returns>A new object that is a copy of this instance with the new value.</returns>
    /// <exception cref="Exception">Occurrs when the value does not represent a string.</exception>
    public IOption Copy(
        object value)
    {
        if (value is not string typedValue)
            throw new Exception("Value does not represent option type 'String'.");

        return new SelectableOption(Name, Description, typedValue, Items);
    }
}