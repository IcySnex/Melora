using Melora.Models;
using Melora.Plugins.Enums;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Melora.Services;

public class MediaEncoder
{
    readonly ILogger<MediaEncoder> logger;
    readonly Config config;

    public MediaEncoder(
        ILogger<MediaEncoder> logger,
        Config config)
    {
        this.logger = logger;
        this.config = config;

        logger.LogInformation("[MediaEncoder-.ctor] MediaEncoder has been initialized");
    }


    Process CreateProcessor(
        string filePath,
        int quality,
        DataReceivedEventHandler onDataRecieved)
    {
        Process processor = new()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = config.Paths.FFmpegLocation,
                Arguments = $"-i - -v quiet -hide_banner -map_metadata -1 -stats -y -b:a {quality}k \"{filePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
            }
        };
        processor.ErrorDataReceived += onDataRecieved;

        logger.LogInformation("[MediaEncoder-CreateProcessor] Created FFmpeg processor...");
        return processor;
    }


    public async Task WriteAsync(
        string filePath,
        Stream stream,
        Quality quality,
        IProgress<TimeSpan> progress,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[MediaEncoder-WriteAsync] Starting to write stream...");

        if (Path.GetDirectoryName(filePath) is string directory && directory.Length != 0)
            Directory.CreateDirectory(directory);


        Process processor = CreateProcessor(filePath, (int)quality, (s, e) =>
        {
            if (e.Data is null || !e.Data.Contains("time="))
                return;

            string? currentTimeInfo = e.Data.Split(' ').FirstOrDefault(info => info.Contains("time="));
            if (currentTimeInfo is null || currentTimeInfo.Length < 16 || !TimeSpan.TryParseExact(currentTimeInfo[5..16], "hh\\:mm\\:ss\\.ff", null, out TimeSpan currentTime))
                return;

            progress.Report(currentTime);
            logger.LogInformation("[MediaEncoder-OnProcessorDataRecieved] Writing stream: {currentTime}", currentTime);
        });

        cancellationToken.Register(async () =>
        {
            try
            {
                processor.Kill();
                await Task.Delay(1000, CancellationToken.None);

                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[MediaEncoder-OnCancellationTokenCanceled] Failed to kill processor and delete file: {exception}", ex.Message);
            }
        });

        processor.Start();
        processor.BeginErrorReadLine();

        using (Stream processorInput = processor.StandardInput.BaseStream)
            await stream.CopyToAsync(processorInput, cancellationToken);

        await processor.WaitForExitAsync(cancellationToken);
        logger.LogInformation("[MediaEncoder-WriteAsync] Finished writing stream...");
    }
}