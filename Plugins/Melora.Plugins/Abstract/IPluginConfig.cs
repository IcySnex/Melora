using Melora.Plugins.Exceptions;
using Melora.Plugins.Models;
using System.Text.Json.Serialization;

namespace Melora.Plugins.Abstract;

/// <summary>
/// Describes a configuration for a plugin
/// </summary>
[JsonDerivedType(typeof(PlatformSupportPluginConfig), typeDiscriminator: "platform-support")]
[JsonDerivedType(typeof(MetadataPluginConfig), typeDiscriminator: "metadata")]
public interface IPluginConfig
{
    /// <summary>
    /// Additional config options for the plugin.
    /// </summary>
    IOption[] Options { get; }

    /// <summary>
    /// Resets the config to the plugins default.
    /// </summary>
    void Reset();
}


/// <summary>
/// Contains extension methods for plugin configs.
/// </summary>
public static class PluginConfigExtensions
{
    /// <summary>
    /// Gets the value of the option with the given name.
    /// </summary>
    /// <param name="config">The config to get the option from.</param>
    /// <param name="name">The name of the requested option.</param>
    /// <returns>The requested options value.</returns>
    /// <exception cref="PluginOptionException">Occurrs when the option could not be found or the option value does not represent the requested type</exception>
    public static T GetOption<T>(
        this IPluginConfig config,
        string name)
    {
        IOption? option = config.Options.FirstOrDefault(option => option.Name == name)
            ?? throw new PluginOptionException(name, new($"Option with given name '{name}' was not found."));

        if (option.Value is not T result)
            throw new PluginOptionException(name, new($"Value does not represent option type '{typeof(T).Name}'."));

        return result;
    }

    /// <summary>
    /// Gets the value of the string option with the given name.
    /// </summary>
    /// <param name="config">The config to get the option from.</param>
    /// <param name="name">The name of the requested option.</param>
    /// <returns>The requested string options value.</returns>
    /// <exception cref="PluginOptionException">Occurrs when the option could not be found or the option value does not represent a string</exception>
    public static string GetStringOption(
        this IPluginConfig config,
        string name) =>
        config.GetOption<string>(name);

    /// <summary>
    /// Gets the value of the int option with the given name.
    /// </summary>
    /// <param name="config">The config to get the option from.</param>
    /// <param name="name">The name of the requested option.</param>
    /// <returns>The requested int options value.</returns>
    /// <exception cref="PluginOptionException">Occurrs when the option could not be found or the option value does not represent a int</exception>
    public static int GetIntOption(
        this IPluginConfig config,
        string name) =>
        config.GetOption<int>(name);

    /// <summary>
    /// Gets the value of the double option with the given name.
    /// </summary>
    /// <param name="config">The config to get the option from.</param>
    /// <param name="name">The name of the requested option.</param>
    /// <returns>The requested double options value.</returns>
    /// <exception cref="PluginOptionException">Occurrs when the option could not be found or the option value does not represent a double</exception>
    public static double GetDoubleOption(
        this IPluginConfig config,
        string name) =>
        config.GetOption<double>(name);

    /// <summary>
    /// Gets the value of the bool option with the given name.
    /// </summary>
    /// <param name="config">The config to get the option from.</param>
    /// <param name="name">The name of the requested option.</param>
    /// <returns>The requested bool options value.</returns>
    /// <exception cref="PluginOptionException">Occurrs when the option could not be found or the option value does not represent a bool</exception>
    public static bool GetBoolOption(
        this IPluginConfig config,
        string name) =>
        config.GetOption<bool>(name);

    /// <summary>
    /// Gets the selected value of the selectable option with the given name.
    /// </summary>
    /// <param name="config">The config to get the option from.</param>
    /// <param name="name">The name of the requested option.</param>
    /// <returns>The requested bool options value.</returns>
    /// <exception cref="PluginOptionException">Occurrs when the option could not be found or the selected option value does not represent a string</exception>
    public static string GetSelectableOption(
        this IPluginConfig config,
        string name) =>
        config.GetOption<string>(name);


    /// <summary>
    /// Checks if the config contains all given options.
    /// </summary>
    /// <param name="config">The config.</param>
    /// <param name="options">The options to check for.</param>
    /// <returns>A bool representing the comparison.</returns>
    public static bool ContainsAll(
        this IPluginConfig config,
        IOption[] options)
    {
        return config.Options.Length == options.Length && config.Options.All(option => options.Any(defaultOption => option.Name == defaultOption.Name));
    }
}