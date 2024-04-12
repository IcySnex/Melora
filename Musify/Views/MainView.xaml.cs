using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Musify.Helpers;
using WinRT.Interop;

namespace Musify.Views;

public sealed partial class MainView : Window
{
    readonly ILogger<MainView> logger;

    readonly IntPtr hWnd;
    readonly WindowId id;
    readonly AppWindow window;
    readonly InputNonClientPointerSource nonClientInputSrc;


    public MainView(
        ILogger<MainView> logger)
    {
        this.logger = logger;

        hWnd = WindowNative.GetWindowHandle(this);
        id = Win32Interop.GetWindowIdFromWindow(hWnd);
        window = AppWindow.GetFromWindowId(id);
        nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(id);

        SystemBackdrop = MicaController.IsSupported() ? new MicaBackdrop() : new DesktopAcrylicBackdrop();
        ExtendsContentIntoTitleBar = true;
        nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, [new(4, 8, 32, 32)]);

        InitializeComponent();

        logger.LogInformation("MainView has been created and intialized");
    }


    public void SetIcon(
        string path)
    {
        window.SetIcon(path);

        logger?.LogInformation("[MainView-SetIcon] Set app icon to path: {path}", path);
    }

    public void SetSize(
        int width,
        int height)
    {
        window.Resize(new(width + 16, height + 39));

        logger.LogInformation("[MainView-SetSize] Set window size: {width}x{height}", width, height);
    }

    public void SetMinSize(
        int width,
        int height)
    {
        IntPtr dpi = Win32.GetDpiForWindow(hWnd);

        Win32.MinWidth = (int)(width * (float)dpi / 96);
        Win32.MinHeight = (int)(height * (float)dpi / 96);

        Win32.NewWndProc = new Win32.WinProc(Win32.NewWindowProc);
        Win32.OldWndProc = IntPtr.Size == 8 ? Win32.SetWindowLongPtr(hWnd, -16 | 0x4 | 0x8, Win32.NewWndProc) : Win32.SetWindowLong(hWnd, -16 | 0x4 | 0x8, Win32.NewWndProc);

        logger.LogInformation("[MainView-SetMinSize] Set minimum window size: {width}x{height}", width, height);
    }
}