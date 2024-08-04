using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Musify.Enums;

namespace Musify.Controls;

public sealed class Notification : Control
{
    public Notification()
    {
        DefaultStyleKey = typeof(Notification);
    }


    protected override async void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        Grid rootLayout = (Grid)GetTemplateChild("RootLayout");
        Button moreButton = (Button)rootLayout.Children[4];
        Button closeButton = (Button)rootLayout.Children[5];

        moreButton.Click += (s, e) =>
            MoreButtonClicked?.Invoke(this, new());
        closeButton.Click += (s, e) =>
            ClosingRequested?.Invoke(this, new());

        UpdateVisualState();

        if (CloseAfter is not null)
        {
            await Task.Delay(CloseAfter.Value);
            ClosingRequested?.Invoke(this, new());
        }
    }


    static void OnLevelChanged(
        DependencyObject sender,
        DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue == e.OldValue)
            return;

        Notification owner = (Notification)sender;
        owner.UpdateVisualState();
    }


    void UpdateVisualState() =>
        VisualStateManager.GoToState(this, Level switch
        {
            NotificationLevel.Information => "Information",
            NotificationLevel.Warning => "Warning",
            NotificationLevel.Error => "Error",
            NotificationLevel.Success => "Success",
            _ => "Information",
        }, true);


    public event EventHandler? MoreButtonClicked;

    public event EventHandler? ClosingRequested;


    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        "Title", typeof(string), typeof(Notification), new PropertyMetadata("This is a title!"));


    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        "Message", typeof(string), typeof(Notification), new PropertyMetadata("This is the message of a notification."));


    public NotificationLevel Level
    {
        get => (NotificationLevel)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    public static readonly DependencyProperty LevelProperty = DependencyProperty.Register(
        "Level", typeof(NotificationLevel), typeof(Notification), new PropertyMetadata(NotificationLevel.Information, OnLevelChanged));


    public TimeSpan? CloseAfter
    {
        get => (TimeSpan?)GetValue(CloseAfterProperty);
        set => SetValue(CloseAfterProperty, value);
    }

    public static readonly DependencyProperty CloseAfterProperty = DependencyProperty.Register(
        "CloseAfter", typeof(TimeSpan?), typeof(Notification), new PropertyMetadata(TimeSpan.FromSeconds(3)));


    public Visibility MoreButtonVisibility
    {
        get => (Visibility)GetValue(MoreButtonVisibilityProperty);
        set => SetValue(MoreButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty MoreButtonVisibilityProperty = DependencyProperty.Register(
        "MoreButtonVisibility", typeof(Visibility), typeof(Notification), new PropertyMetadata(Visibility.Collapsed));
}