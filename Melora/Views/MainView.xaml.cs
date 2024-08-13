using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Melora.Controls;
using Melora.Enums;
using Melora.Helpers;
using Melora.Models;
using Melora.Plugins.Abstract;
using Melora.Services;
using Windows.Foundation;
using WinRT.Interop;

namespace Melora.Views;

public sealed partial class MainView : Window
{
    readonly ILogger<MainView> logger;
    readonly Config config;
    readonly PluginManager pluginManager;
    readonly JsonConverter jsonConverter;

    public MainView(
        ILogger<MainView> logger,
        Config config,
        PluginManager pluginManager,
        JsonConverter jsonConverter)
    {
        this.logger = logger;
        this.config = config;
        this.pluginManager = pluginManager;
        this.jsonConverter = jsonConverter;

        pluginManager.Subscribe<PlatformSupportPlugin>(
            plugin =>
            {
                NavigationViewItem pluginItem = new()
                {
                    Content = plugin.Name,
                    Icon = new PathIcon() { Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), plugin.IconPathData) },
                    Tag = plugin
                };
                NavigationView.MenuItems.Insert(NavigationView.MenuItems.Count - 3, pluginItem);
            },
            plugin =>
            {
                NavigationViewItem? pluginItem = NavigationView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(item => (string)item.Content == plugin.Name);
                NavigationView.MenuItems.Remove(pluginItem);
            });

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


    readonly IntPtr hWnd;
    readonly WindowId id;
    readonly AppWindow window;
    readonly InputNonClientPointerSource nonClientInputSrc;


    void OnClosed(object _, WindowEventArgs _1)
    {
        foreach (IPlugin plugin in pluginManager.LoadedPlugins)
            config.PluginBundles.Configs[plugin.GetType().Name] = plugin.Config;

        string jsonConfig = jsonConverter.ToString(config);
        File.WriteAllText("Config.json", jsonConfig);

        LoggerView?.Close();

        logger.LogInformation("[MainView-Closed] Closed main window.");
    }

    void OnRootLayoutPreviewKeyDown(object _, KeyRoutedEventArgs e)
    {
        if (LoadingPopupRootLayout.IsHitTestVisible)
            e.Handled = true;
    }

    void OnLoadingPopupCancelButtonClicked(object _, RoutedEventArgs _1)
    {
        onCancelButtonClicked?.Invoke();
        HideLoadingPopup();
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

    public void SetSize(
        int width,
        int height,
        Window window)
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(window);
        WindowId id = Win32Interop.GetWindowIdFromWindow(hWnd);
        AppWindow appWindow = AppWindow.GetFromWindowId(id);

        appWindow.Resize(new(width + 16, height + 39));

        logger.LogInformation("[MainView-SetSize] Set window size for external window: {width}x{height}", width, height);
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


    public IAsyncOperation<ContentDialogResult> AlertAsync(
        ContentDialog dialog)
    {
        foreach (Popup popup in VisualTreeHelper.GetOpenPopupsForXamlRoot(Content.XamlRoot))
            if (popup.Child is ContentDialog openDialog)
                openDialog.Hide();

        dialog.XamlRoot = Content.XamlRoot;
        dialog.PrimaryButtonStyle = (Style)App.Current.Resources["AccentButtonStyle"];
        dialog.Style = (Style)App.Current.Resources["DefaultContentDialogStyle"];

        logger.LogInformation("[MainView-AlertAsync] ContentDialog was requested");
        return dialog.ShowAsync();
    }

    public IAsyncOperation<ContentDialogResult> AlertAsync(
        object content,
        string? title = null,
        string? closeButton = "Okay",
        string? primaryButton = null)
    {
        ContentDialog dialog = new()
        {
            Content = content,
            Title = title,
            CloseButtonText = closeButton,
            PrimaryButtonText = primaryButton
        };
        return AlertAsync(dialog);
    }


    public void ShowNotification(
        string title,
        string message,
        NotificationLevel level,
        Action? moreButtonClicked)
    {
        if (NotificationsContainer.Children.Count > 5)
            NotificationsContainer.Children.RemoveAt(0);

        Notification notification = new()
        {
            Title = title,
            Message = message,
            Level = level,
            CloseAfter = level switch
            {
                NotificationLevel.Information => TimeSpan.FromSeconds(3),
                NotificationLevel.Warning => TimeSpan.FromSeconds(5),
                NotificationLevel.Error => TimeSpan.FromSeconds(10),
                NotificationLevel.Success => TimeSpan.FromSeconds(3),
                _ => null
            }
        };
        notification.ClosingRequested += (s, e) =>
            NotificationsContainer.Children.Remove(notification);

        if (moreButtonClicked is not null)
        {
            notification.MoreButtonVisibility = Visibility.Visible;
            notification.MoreButtonClicked += (s, e) =>
            {
                NotificationsContainer.Children.Remove(notification);
                moreButtonClicked.Invoke();
            };
        }

        logger.LogInformation("[MainView-ShowNotification] Notification was requested");
        NotificationsContainer.Children.Add(notification);
    }

    public void ShowNotification(
        string title,
        string message,
        NotificationLevel level = NotificationLevel.Information,
        string? innerMessage = null) =>
        ShowNotification(
            title,
            message,
            level,
            innerMessage is null ? null : async () => await AlertAsync($"{message}\n\n{innerMessage}", title));


    public LoggerView? LoggerView = null;

    public void CreateLoggerView()
    {
        if (LoggerView is not null)
        {
            LoggerView.Activate();
            return;
        }

        LoggerView = new();

        App.Sink.OnNewLog += LoggerView.OnNewLog;
        LoggerView.Closed += (s, e) =>
        {
            App.Sink.OnNewLog -= LoggerView.OnNewLog;
            LoggerView = null;
        };

        SetSize(800, 400, LoggerView);
        LoggerView.Activate();

        logger.LogInformation("[MainView-CreateLoggerView] Created new LoggerView and hooked handler");
    }


    Action? onCancelButtonClicked;

    public IProgress<string> ShowLoadingPopup(
        Action? onCancelButtonClicked = null)
    {
        LoadingPopupRootLayout.Opacity = 1;
        LoadingPopupRootLayout.IsHitTestVisible = true;
        this.onCancelButtonClicked = onCancelButtonClicked;

        return new Progress<string>(message => LoadingPopupTitleBlock.Text = message);
    }

    public void HideLoadingPopup()
    {
        LoadingPopupRootLayout.Opacity = 0;
        LoadingPopupRootLayout.IsHitTestVisible = false;
        onCancelButtonClicked = null;
    }


    public void Initialize(
        object target)
    {
        InitializeWithWindow.Initialize(target, hWnd);

        logger.LogInformation("[MainView-Initialize] Target was initialized with window");
    }
}