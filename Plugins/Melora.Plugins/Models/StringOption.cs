using CommunityToolkit.Mvvm.ComponentModel;
using Melora.Plugins.Abstract;
using System.Text.Json.Serialization;

namespace Melora.Plugins.Models;

/// <summary>
/// Represents a custom string option for a plugin config.
/// </summary>
/// <remarks>
/// Creates a new StringOption.
/// </remarks>
/// <param name="name">The name of the option.</param>
/// <param name="description">The description of the option.</param>
/// <param name="value">The value of the option.</param>
/// <param name="maxLength">The max length of the value</param>
/// <param name="isObscured">Weither the value should be obscured in the UI.</param>
public partial class StringOption(
    string name,
    string description,
    string value,
    int maxLength = 0,
    bool isObscured = false) : ObservableObject, IOption
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
    string value = value;
    object IOption.Value
    {
        get => Value;
        set => Value = (string)value;
    }

    /// <summary>
    /// The max length of the value.
    /// </summary>
    [JsonIgnore]
    public int MaxLength { get; } = maxLength;

    /// <summary>
    /// Weither the value should be obscured in the UI.
    /// </summary>
    [JsonIgnore]
    public bool IsObscured { get; } = isObscured;


    /// <summary>
    /// Creates a new object that is a copy of the current instance with the new value.
    /// </summary>
    /// <returns>A new object that is a copy of this instance with the new value.</returns>
    public IOption Copy(
        object value)
    {
        if (value is not string typedValue)
            throw new Exception("Value does not represent option type 'String'.");

        return new StringOption(Name, Description, typedValue, MaxLength, IsObscured);
    }
}