using Melora.Plugins.Models;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Melora.Plugins.Abstract;

/// <summary>
/// Represents a custom option for a plugin config.
/// </summary>
[JsonDerivedType(typeof(StringOption), typeDiscriminator: "string")]
[JsonDerivedType(typeof(BoolOption), typeDiscriminator: "bool")]
[JsonDerivedType(typeof(IntOption), typeDiscriminator: "int")]
[JsonDerivedType(typeof(DoubleOption), typeDiscriminator: "double")]
[JsonDerivedType(typeof(SelectableOption), typeDiscriminator: "selectable")]
public interface IOption : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <summary>
    /// The name of the option.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The description of the option.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// The value of the option.
    /// </summary>
    object Value { get; set; }


    /// <summary>
    /// Creates a new object that is a copy of the current instance with the new value.
    /// </summary>
    /// <param name="value">The new value of the copy.</param>
    /// <returns>A new object that is a copy of this instance with the new value.</returns>
    /// <exception cref="Exception">Occurrs when the value does not represent the option kind.</exception>
    public IOption Copy(
        object value);
}