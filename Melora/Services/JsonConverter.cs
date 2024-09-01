using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Melora.Services;

public class JsonConverter
{
    readonly ILogger<JsonConverter> logger;

    public JsonConverter(
        ILogger<JsonConverter> logger)
    {
        this.logger = logger;

        logger.LogInformation("[JsonConverter-.ctor] JsonConverter has been initialized");
    }


    public string ToString(
        object input)
    {
        logger.LogInformation("[JsonConverter-ToString] Serializing object to string");
        return JsonSerializer.Serialize(input);
    }

    public T? ToObject<T>(
        string input)
    {
        logger.LogInformation("[JsonConverter-ToObject] Deserializing string to object");
        return JsonSerializer.Deserialize<T>(input);
    }
}