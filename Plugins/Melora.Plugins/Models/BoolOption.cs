using CommunityToolkit.Mvvm.ComponentModel;
using Melora.Plugins.Abstract;
using System.Text.Json.Serialization;

namespace Melora.Plugins.Models;

/// <summary>
/// Represents a custom bool option for a plugin config.
/// </summary>
/// <remarks>
/// Creates a new BoolOption.
/// </remarks>
/// <param name="name">The name of the option.</param>
/// <param name="description">The description of the option.</param>
/// <param name="value">The value of the option.</param>
public partial class BoolOption(
    string name,
    string description,
    bool value) : ObservableObject, IOption
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

    [ObservableProperty]
    bool value = value;
    object IOption.Value
    {
        get => Value;
        set => Value = (bool)value;
    }


    /// <summary>
    /// Creates a new object that is a copy of the current instance with the new value.
    /// </summary>
    /// <returns>A new object that is a copy of this instance with the new value.</returns>
    public IOption Copy(
        object value)
    {
        if (value is not bool typedValue)
            throw new Exception("Value does not represent option type 'Bool'.");

        return new BoolOption(Name, Description, typedValue);
    }
}