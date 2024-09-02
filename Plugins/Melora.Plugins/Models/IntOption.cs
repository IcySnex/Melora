using CommunityToolkit.Mvvm.ComponentModel;
using Melora.Plugins.Abstract;
using System.Text.Json.Serialization;

namespace Melora.Plugins.Models;

/// <summary>
/// Represents a custom int option for a plugin config.
/// </summary>
/// <remarks>
/// Creates a new IntOption.
/// </remarks>
/// <param name="name">The name of the option.</param>
/// <param name="description">The description of the option.</param>
/// <param name="value">The value of the option.</param>
/// <param name="minimum">The minimum the value needs to be</param>
/// <param name="maximum">The maximum the value can be.</param>
public partial class IntOption(
    string name,
    string description,
    int value,
    int minimum = int.MinValue,
    int maximum = int.MaxValue) : ObservableObject, IOption
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
    int value = value;
    object IOption.Value
    {
        get => Value;
        set => Value = (int)value;
    }

    /// <summary>
    /// The minimum the value needs to be.
    /// </summary>
    [JsonIgnore]
    public int Minimum { get; } = minimum;

    /// <summary>
    /// The maximum the value can be.
    /// </summary>
    [JsonIgnore]
    public int Maximum { get; } = maximum;


    /// <summary>
    /// Creates a new object that is a copy of the current instance with the new value.
    /// </summary>
    /// <returns>A new object that is a copy of this instance with the new value.</returns>
    public IOption Copy(
        object value)
    {
        if (value is not int typedValue)
            throw new Exception("Value does not represent option type 'Int'.");

        return new IntOption(Name, Description, typedValue, Minimum, Maximum);
    }
}