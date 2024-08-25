using Melora.Models;
using Melora.Views;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Melora.Services;

public class UpdateManager
{
    public readonly static string Architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

    public readonly static Version CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new(1, 0, 0);


    readonly ILogger<UpdateManager> logger;
    readonly Config config;
    readonly MainView mainView;
    readonly JsonConverter converter;

    readonly HttpClient client;

    public UpdateManager(
        ILogger<UpdateManager> logger,
        Config config,
        MainView mainView,
        JsonConverter converter)
    {
        this.logger = logger;
        this.config = config;
        this.mainView = mainView;
        this.converter = converter;

        client = new();
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"Melora/{CurrentVersion} (https://icysnex.github.io/Melora/)");

        logger.LogInformation("[UpdateManager-.ctor] UpdateManager has been initialized");
    }


    public async Task<Release> GetLatestReleaseAsync(
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[UpdateManager-GetLatestReleaseAsync] Checking for updates...");
        string releasesData = await client.GetStringAsync(config.Updates.ReleasesUrl, cancellationToken);

        Release[] releases = converter.ToObject<Release[]>(releasesData)?? throw new Exception("Failed to parse releases data. Make sure the provided update server supports the GitHub releases format.");
        return releases
            .Where(release => release.Channel == config.Updates.Channel && release.Binary is not null)
            .OrderByDescending(release => release.Version)
            .FirstOrDefault() ?? throw new Exception($"No valid releases found for channel '{config.Updates.Channel}' with a downloadable binary for this architecture ({Architecture}).");
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
        string binaryPath = Path.Combine(tempDirectory, "melora_update_binary.tmp");
        string scriptPath = Path.Combine(tempDirectory, "melora_update_script.bat");

        string scriptContent = $"""
            @echo off

            set maxTries=10
            set tryCount=1


            :loop
            echo Checking if Melora did exit [%tryCount%/%maxTries%]...

            tasklist | find "Melora.exe" >nul 2>&1
            if errorlevel 1 goto replace
                        
            set /a tryCount+=1
            if %tryCount% gtr %maxTries% goto timeout
                        
            timeout /t 1 >nul
            goto loop


            :replace
            echo Replacing Melora now...

            move /Y "{binaryPath}" "Melora.exe" >nul
            start "" "Melora.exe" >nul
            del "%~f0"


            :timeout
            echo Timeout reached: Update not installed. Melora didn't exit in the expected time.
            powershell -Command "Add-Type -AssemblyName System.Windows.Forms; [System.Windows.Forms.MessageBox]::Show('Timeout reached: Melora didn''t exit in the expected time.', 'Melora: Update failed', [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)"

            del "%~f0"
            """;
        await File.WriteAllTextAsync(scriptPath, scriptContent, cancellationToken);

        cancellationToken.Register(async () =>
        {
            await Task.Delay(1000, CancellationToken.None);

            if (File.Exists(scriptPath))
                File.Delete(scriptPath);
            if (File.Exists(binaryPath))
                File.Delete(binaryPath);
        });


        logger.LogInformation("[UpdateManager-InstallReleaseAsync] Downloading new release...");
        progress.Report("Downloading new release...");

        using Stream binaryStream = await client.GetStreamAsync(release.Binary.DownloadUrl, cancellationToken);
        using Stream fileStream = File.Open(binaryPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

        await binaryStream.CopyToAsync(fileStream, cancellationToken);


        int countdownSeconds = 5;
        for (int i = countdownSeconds; i > 0; i--)
        {
            logger.LogInformation("[UpdateManager-InstallReleaseAsync] Restarting in {seoncdsLeft} seconds", i);
            progress.Report($"Restarting in {i} seconds...");

            await Task.Delay(1000, cancellationToken);
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = scriptPath,
            CreateNoWindow = true,
            UseShellExecute = false
        };
        Process.Start(startInfo);

        mainView.Close();
        Application.Current.Exit();
    }
}