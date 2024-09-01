using Melora.Helpers;
using Melora.Models;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Melora.Services;

public class UpdateManager
{
    public readonly static Version Version = typeof(App).Assembly.GetName().Version.Normalize();

    public readonly static string Architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

#if DEBUG
    public readonly static string BuildMode = "Debug";
#else
    public readonly static string BuildMode = "Release";
#endif

    public readonly static Version RuntimeVersion = Environment.Version.Normalize();

    public readonly static Version WindowsAppSDKVersion = typeof(AppInstance).Assembly.GetName().Version.Normalize();



    readonly ILogger<UpdateManager> logger;
    readonly Config config;
    readonly JsonConverter converter;

    readonly HttpClient client;

    public UpdateManager(
        ILogger<UpdateManager> logger,
        Config config,
        JsonConverter converter)
    {
        this.logger = logger;
        this.config = config;
        this.converter = converter;

        client = new();
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"Melora/{Version} (https://icysnex.github.io/Melora/)");

        logger.LogInformation("[UpdateManager-.ctor] UpdateManager has been initialized");
    }


    public async Task<Release?> GetLatestReleaseAsync(
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[UpdateManager-GetLatestReleaseAsync] Checking for updates...");
        string releasesData = await client.GetStringAsync(config.Updates.ReleasesUrl, cancellationToken);

        Release[] releases = converter.ToObject<Release[]>(releasesData) ?? throw new Exception("Failed to parse releases data. Make sure the provided update server supports the GitHub releases format.");
        return releases
            .Where(release => release.Channel == config.Updates.Channel && release.Binary is not null)
            .OrderByDescending(release => release.Version)
            .FirstOrDefault();
    }


    public async Task InstallReleaseAsync(
        Release release,
        IProgress<string> progress,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[UpdateManager-InstallReleaseAsync] Preparing release install...");
        progress.Report("Preparing release install...");

        if (release.Binary is null)
            throw new NullReferenceException("Release binary is null.");

        string tempDirectory = Path.GetTempPath();
        string scriptPath = Path.Combine(tempDirectory, "melora_update_script.ps1");
        string binariesPath = Path.Combine(tempDirectory, "melora_update_binaries.zip");
        string binariesDirectory = Path.Combine(tempDirectory, "melora_update_binaries");

        string scriptContent = $$"""
            Add-Type -AssemblyName System.Windows.Forms


            # Wait until Melora is closed
            $maxTries = 10
            $tryCount = 1

            while ($tryCount -le $maxTries) {
                Write-Output "Checking if Melora is closed [$tryCount/$maxTries]..."

                if (-not (Get-Process -Name "Melora" -ErrorAction SilentlyContinue)) {
                    break
                }

                Start-Sleep -Seconds 1
                $tryCount++
            }
            if ($tryCount -gt $maxTries) {
                [System.Windows.Forms.MessageBox]::Show("Timeout reached: Melora didn't exit in the expected time.", "Melora Update: Error", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
                exit 1
            }
            

            # Clean up old Melora files
            $result = [System.Windows.Forms.MessageBox]::Show("Do you want to clean up old Melora files?`n`nCaution: this operation will delete ALL files in the directory Melora is located in except for your config, plugins, logs & backups.", "Melora Update: Warning", [System.Windows.Forms.MessageBoxButtons]::YesNo, [System.Windows.Forms.MessageBoxIcon]::Exclamation)
                        
            if ($result -eq "No") {
                Write-Output "Skipping clean up of old files..."
            } else {
                Write-Output "Cleaning up old Melora files..."
                Get-ChildItem -Path "." -File | Where-Object { $_.Name -ne "Config.json" } | Remove-Item -Force
                Get-ChildItem -Path "." -Directory | Where-Object { $_.Name -notin @("Plugins", "Logs", "Backup") } | Remove-Item -Recurse -Force
            }
            

            # Copy new files and start Melora
            Write-Output "Copying new Melora files..."
            Copy-Item -Path (Join-Path {{binariesDirectory}} "*") -Destination "." -Recurse -Force
            

            # Cleanup
            Write-Output "Cleaning up temporary files..."
            Remove-Item -Path {{binariesDirectory}} -Recurse -Force
            Remove-Item -Path {{scriptPath}} -Force
            

            # Start Melora
            Write-Output "Starting Melora..."
            [System.Windows.Forms.MessageBox]::Show("Updating Melora finished successfully. Opening Melora now...", "Melora Update: Sucess", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Information)
            Start-Process "Melora.exe" -ArgumentList "--updated"
            """;
        await File.WriteAllTextAsync(scriptPath, scriptContent, cancellationToken);

        cancellationToken.Register(async () =>
        {
            try
            {
                await Task.Delay(1000, CancellationToken.None);

                File.Delete(scriptPath);
                File.Delete(binariesPath);
                Directory.Delete(binariesDirectory, true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[UpdateManager-InstallReleaseAsync] Failed to delete file/directory: {exception}", ex.Message);
            }
            await Task.Delay(1000, CancellationToken.None);
        });


        logger.LogInformation("[UpdateManager-InstallReleaseAsync] Downloading new release...");
        progress.Report("Downloading new release...");

        using (Stream binariesStream = await client.GetStreamAsync(release.Binary.DownloadUrl, cancellationToken))
        using (Stream fileStream = File.Open(binariesPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            await binariesStream.CopyToAsync(fileStream, cancellationToken);


        logger.LogInformation("[UpdateManager-InstallReleaseAsync] Extracting release...");
        progress.Report("Extracting release...");

        if (Directory.Exists(binariesDirectory))
            Directory.Delete(binariesDirectory, true);
        await Task.Run(() => ZipFile.ExtractToDirectory(binariesPath, binariesDirectory), cancellationToken);

        File.Delete(binariesPath);


        int countdownSeconds = 5;
        for (int i = countdownSeconds; i > 0; i--)
        {
            logger.LogInformation("[UpdateManager-InstallReleaseAsync] Restarting in {seoncdsLeft} seconds", i);
            progress.Report($"Restarting in {i} seconds...");

            await Task.Delay(1000, cancellationToken);
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = "powershell.exe",
            Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\"",
            CreateNoWindow = true
        };
        Process.Start(startInfo);

        Application.Current.Exit();
    }
}