using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Musify.Views;

public sealed partial class LoggerView : Window
{
    public LoggerView()
    {
        SystemBackdrop = MicaController.IsSupported() ? new MicaBackdrop() : new DesktopAcrylicBackdrop();

        InitializeComponent();
    }
}