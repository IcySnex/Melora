using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musify.Models;
using System;
using System.Net;
using System.Text;

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


    static void ExtractText(
        HtmlNode node,
        StringBuilder builder)
    {
        foreach (HtmlNode childNode in node.ChildNodes)
            switch (childNode.NodeType)
            {
                case HtmlNodeType.Text:
                    builder.Append(childNode.InnerText);
                    break;
                case HtmlNodeType.Element:
                    if (childNode.Name == "br")
                        builder.AppendLine();
                    else
                        ExtractText(childNode, builder);
                    break;
            }
    }

    public async Task<string> GetAsync(
        string url,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[Lyrics-GetAsync] Downloading lyrics...");
        progress?.Report("Downloading lyrics...");

        HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        logger.LogInformation("[Lyrics-GetAsync] Loading lyrics...");
        progress?.Report("Loading lyrics...");

        Stream body = await response.Content.ReadAsStreamAsync(cancellationToken);
        HtmlDocument html = new();
        html.Load(body);

        logger.LogInformation("[Lyrics-GetAsync] Parsing lyrics...");
        progress?.Report("Parsing lyrics...");

        HtmlNodeCollection nodes = html.DocumentNode.SelectNodes("//div[@data-lyrics-container]");
        if (nodes is null || nodes.Count == 0)
        {
            logger.LogError("[Lyrics-GetAsync] Failed to get lyrics: Parsed HTML nodes is null or empty");
            throw new NullReferenceException("Parsed HTML nodes is null or empty.");
        }

        StringBuilder builder = new();
        foreach (HtmlNode node in nodes)
        {
            ExtractText(node, builder);
            builder.AppendLine();
        }

        return WebUtility.HtmlDecode(builder.ToString().TrimEnd());
    }
}