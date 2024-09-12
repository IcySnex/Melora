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


    static readonly char[] invalidFileNameChars =
        [
            '\"', '<', '>', '|', '\0',
            (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
            (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
            (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
            (char)31, ':', '*', '?', '/'
        ];

    public static string ToLegitFileName(
        this string input)
    {
        foreach (char c in invalidFileNameChars)
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