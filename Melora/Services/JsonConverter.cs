using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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


    readonly JsonSerializerSettings settings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto
    };


    public string ToString(
        object input)
    {
        logger.LogInformation("[JsonConverter-ToString] Serializing object to string");
        return JsonConvert.SerializeObject(input, settings);
    }

    public T? ToObject<T>(
        string input)
    {
        logger.LogInformation("[JsonConverter-ToObject] Deserializing string to object");
        return JsonConvert.DeserializeObject<T>(input, settings);
    }
}