using System.Text;

namespace Musify.Helpers;

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
        builder.AppendLine(ex.Message.Length <= 100 ? $"> {ex.Message}" : $"> {ex.Message[..100]}...");

        if (ex.InnerException is not null)
            FormatException(builder, ex.InnerException, level + 1);
    }
}