using System.Text.Json.Serialization;
using System.Text.Json;

namespace Musify.Json;

public class DateComponentsConverter : JsonConverter<DateTime>
{
    class ReleaseDateComponents
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("month")]
        public int Month { get; set; }

        [JsonPropertyName("day")]
        public int Day { get; set; }
    }


    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        ReleaseDateComponents? dateComponents = JsonSerializer.Deserialize<ReleaseDateComponents>(ref reader, options);
        if (dateComponents is null)
            return DateTime.MinValue;

        return new(dateComponents.Year, dateComponents.Month, dateComponents.Day);
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTime value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("year", value.Year);
        writer.WriteNumber("month", value.Month);
        writer.WriteNumber("day", value.Day);
        writer.WriteEndObject();
    }
}
