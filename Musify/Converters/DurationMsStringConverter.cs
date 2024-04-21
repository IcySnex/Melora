using Microsoft.UI.Xaml.Data;
using System.Text;

namespace Musify.Converters;

public class DurationMsStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds((int)value);
        StringBuilder sb = new();

        if (timeSpan.Hours != 0)
            sb.Append($"{timeSpan.Hours}h");

        if (timeSpan.Minutes != 0)
        {
            if (sb.Length > 0)
                sb.Append(", ");

            sb.Append($"{timeSpan.Minutes}m");
        }
        if (timeSpan.Seconds != 0)
        {
            if (sb.Length > 0)
                sb.Append(", ");

            sb.Append($"{timeSpan.Seconds}s");
        }

        string result = sb.ToString();
        return string.IsNullOrEmpty(result) ? "N/A" : result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}