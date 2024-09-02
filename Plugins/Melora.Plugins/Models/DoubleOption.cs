using CommunityToolkit.Mvvm.ComponentModel;
using Melora.Plugins.Abstract;
using System.Text.Json.Serialization;

namespace Melora.Plugins.Models;

/// <summary>
/// Represents a custom double option for a plugin config.
/// </summary>
/// <remarks>
/// Creates a new DoubleOption.
/// </remarks>
/// <param name="name">The name of the option.</param>
/// <param name="description">The description of the option.</param>
/// <param name="value">The value of the option.</param>
/// <param name="minimum">The minimum the value needs to be</param>
/// <param name="maximum">The maximum the value can be.</param>
public partial class DoubleOption(
    string name,
    string description,
    double value,
    double minimum = double.MinValue,
    double maximum = double.MaxValue) : ObservableObject, IOption
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
    double value = value;
    object IOption.Value
    {
        get => Value;
        set => Value = (double)value;
    }

    /// <summary>
    /// The minimum the value needs to be.
    /// </summary>
    [JsonIgnore]
    public double Minimum { get; } = minimum;

    /// <summary>
    /// The maximum the value can be.
    /// </summary>
    [JsonIgnore]
    public double Maximum { get; } = maximum;


    /// <summary>
    /// Creates a new object that is a copy of the current instance with the new value.
    /// </summary>
    /// <returns>A new object that is a copy of this instance with the new value.</returns>
    public IOption Copy(
        object value)
    {
        if (value is not double typedValue)
            throw new Exception("Value does not represent option type 'Double'.");

        return new DoubleOption(Name, Description, typedValue, Minimum, Maximum);
    }
}