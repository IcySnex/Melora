using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Serilog.Events;
using System.Text;

namespace Melora.Helpers;

public static class Extensions
{
    public static string ToFormattedString(
        this Exception ex,
        string? message = null)
    {
        StringBuilder builder = new();
        if (message is not null)
        {
            builder.AppendLine(message);
            builder.AppendLine();
        }
        builder.AppendLine("Exception:");

        FormatException(builder, ex, 0);

        return builder.ToString();
    }

    static void FormatException(
        StringBuilder builder,
        Exception ex,
        int level)
    {
        if (ex == null)
            return;

        builder.Append(new string(' ', level * 2));
        builder.AppendLine(ex.Message.Length <= 200 ? $"> {ex.Message}" : $"> {ex.Message[..100]}...");

        if (ex.InnerException is not null)
            FormatException(builder, ex.InnerException, level + 1);
    }


    public static string ToLegitFileName(
        this string input)
    {
        foreach (char c in Path.GetInvalidFileNameChars().Except(['\\']))
            input = input.Replace(c, '_');

        return input;
    }


    public static Paragraph AddHiglightedText(
        this Paragraph paragraph,
        string text,
        SolidColorBrush brush)
    {
        paragraph.Inlines.Add(new Run()
        {
            Text = text,
            Foreground = brush
        });
        return paragraph;
    }


    public static string ToAbbreviation(
        this LogEventLevel level) => level switch
        {
            LogEventLevel.Verbose => "VRB",
            LogEventLevel.Debug => "DBG",
            LogEventLevel.Information => "INF",
            LogEventLevel.Warning => "WRN",
            LogEventLevel.Error => "ERR",
            LogEventLevel.Fatal => "FTL",
            _ => "UNK"
        };


    public static Version Normalize(
        this Version? version) =>
        version is null ? new(1, 0, 0) : version.Revision == -1 ? version : new(version.Major, version.Minor, version.Build);
}