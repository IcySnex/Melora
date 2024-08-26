using Microsoft.UI.Xaml.Controls;
using System.Reflection;

namespace Melora.Views;

public sealed partial class AboutView : Page
{
    public AboutView()
    {
        InitializeComponent();

#if DEBUG
        BuildMode = "Debug";
#else
    BuildMode = "Release";
#endif
        RuntimeVersion = Environment.Version;
        WindowsAppSDKVersion = Assembly.Load("Microsoft.WindowsAppRuntime.Bootstrap.Net").GetName().Version ?? new Version(1, 0, 0, 0);
        WindowsOSVersion = Environment.OSVersion.Version;
    }


    string BuildMode { get; }

    Version RuntimeVersion { get; }

    Version WindowsAppSDKVersion { get; }

    Version WindowsOSVersion { get; }
}