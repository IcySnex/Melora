using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Melora.Plugins.Models;

/// <summary>
/// Represnts a single item of a plugin config.
/// </summary>
/// <remarks>
/// Creates a new PluginConfigItem
/// </remarks>
/// <param name="name">The name of the property.</param>
/// <param name="description">The description of the property.</param>
/// <param name="value">The value of the property.</param>
/// <param name="type">The type of the value. Null if automatically.</param>
[JsonConverter(typeof(PluginConfigItemConverter))]
public partial class PluginConfigItem(
    string name,
    string description,
    object? value,
    string? type = null) : ObservableObject, ICloneable
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
    object? value = value;

    /// <summary>
    /// The type of the value.
    /// </summary>
    public string Type { get; } =
        type is not null ? type : value?.GetType().FullName ?? "Null";


    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    public object Clone() =>
        new PluginConfigItem(Name, Description, Value, Type);
}


internal class PluginConfigItemConverter : JsonConverter<PluginConfigItem>
{
    public override PluginConfigItem Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement root = doc.RootElement;

        string type = root.GetProperty("$type").GetString() ?? throw new JsonException("Could not type discrimnator '$type'.");
        Type valueType = Type.GetType(type) ?? typeof(object);

        string name = root.GetProperty("Name").GetString() ?? throw new JsonException("Could not find property 'Name'.");
        string description = root.GetProperty("Description").GetString() ?? throw new JsonException("Could not find property 'Description'.");

        JsonElement valueElement = root.GetProperty("Value");
        object? value = valueElement.ValueKind == JsonValueKind.Null ? null : JsonSerializer.Deserialize(valueElement.GetRawText(), valueType, options) ?? $"Could not deserialize value using type '{type}'.";

        return new PluginConfigItem(name, description, value, type);
    }

    public override void Write(
        Utf8JsonWriter writer,
        PluginConfigItem value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("$type", value.Type);

        writer.WriteString("Name", value.Name);
        writer.WriteString("Description", value.Description);

        writer.WritePropertyName("Value");
        if (value.Value is null)
            writer.WriteNullValue();
        else
            JsonSerializer.Serialize(writer, value.Value, value.Value.GetType(), options);

        writer.WriteEndObject();
    }
}