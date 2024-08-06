using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Musify.Helpers;
using Serilog.Events;

namespace Musify.Views;

public sealed partial class LoggerView : Window
{
    readonly SolidColorBrush bracketsBrush;
    readonly SolidColorBrush timeBrush;
    readonly SolidColorBrush eventBrush;
    readonly SolidColorBrush sourceBrush;
    readonly SolidColorBrush textBrush;
    readonly SolidColorBrush errorTextBrush;

    public LoggerView()
    {
        SystemBackdrop = MicaController.IsSupported() ? new MicaBackdrop() : new DesktopAcrylicBackdrop();

        InitializeComponent();

        bracketsBrush = (SolidColorBrush)ContentBlock.Resources["BracketsBrush"];
        timeBrush = (SolidColorBrush)ContentBlock.Resources["TimeColor"];
        eventBrush = (SolidColorBrush)ContentBlock.Resources["EventColor"];
        sourceBrush = (SolidColorBrush)ContentBlock.Resources["SourceColor"];
        textBrush = (SolidColorBrush)ContentBlock.Resources["TextColor"];
        errorTextBrush = (SolidColorBrush)ContentBlock.Resources["ErrorTextColor"];
    }


    public void OnNewLog(object? _, LogEvent logEvent)
    {
        string message = logEvent.RenderMessage();
        int sourceEndIndex = message.IndexOf(']');

        Paragraph paragraph = new Paragraph()
            .AddHiglightedText("[", bracketsBrush)
            .AddHiglightedText($"{logEvent.Timestamp:HH:mm:ss} ", timeBrush)
            .AddHiglightedText(logEvent.Level.ToAbbreviation(), eventBrush)
            .AddHiglightedText("] [", bracketsBrush)
            .AddHiglightedText(message[1..sourceEndIndex], sourceBrush)
            .AddHiglightedText("] ", bracketsBrush)
            .AddHiglightedText(message[(sourceEndIndex + 2)..], textBrush);

        if (logEvent.Exception is not null)
            paragraph = paragraph.AddHiglightedText($"\n{logEvent.Exception}", errorTextBrush);

        ContentBlock.Blocks.Insert(0, paragraph);
    }
}