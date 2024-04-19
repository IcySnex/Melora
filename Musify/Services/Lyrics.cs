using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musify.Models;
using System.Net;

namespace Musify.Services;

public class Lyrics
{
    readonly ILogger<Lyrics> logger;
    readonly Config config;
    readonly JsonConverter converter;

    readonly HttpClient client = new();

    public Lyrics(
        ILogger<Lyrics> logger,
        IOptions<Config> config,
        JsonConverter converter)
    {
        this.logger = logger;
        this.config = config.Value;
        this.converter = converter;

        logger.LogInformation("[Lyrics-.ctor] Lyrics has been initialized");
    }


    public async Task<LyricsHit[]> SearchAsync(
        string query,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[Lyrics-SearchAsync] Preparing request...");
        progress?.Report("Preparing request...");

        client.DefaultRequestHeaders.Authorization = new("Bearer", config.Advanced.GeniusAccessToken);
        string requestUrl = $"https://api.genius.com/search?q={WebUtility.UrlEncode(query)}";

        logger.LogInformation("[Lyrics-SearchAsync] Searching for lyrics...");
        progress?.Report("Searching for lyrics...");

        HttpResponseMessage response = await client.GetAsync(requestUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        logger.LogInformation("[Lyrics-SearchAsync] Parsing search results...");
        progress?.Report("Parsing search results...");

        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        LyricsRequestResult? result = converter.ToObject<LyricsRequestResult>(body);
        if (result is null)
        {
            logger.LogError("[Lyrics-SearchAsync] Failed to search for lyrics: Parsed search result is null");
            throw new NullReferenceException("Parsed search result is null.");
        }
        if (result.Meta.StatusCode != 200)
        {
            logger.LogError("[Lyrics-SearchAsync] Failed to search for lyrics: {statusCode}, {message}", result.Meta.StatusCode, result.Meta.Message);
            throw new Exception($"Lyrics request did not return successful status code: {result.Meta.StatusCode}.", new(result.Meta.Message ?? "Unknown message."));
        }

        return result.Response.Hits;
    }
}