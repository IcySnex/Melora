using Microsoft.UI.Xaml.Data;

namespace Melora.Converters;

public class TimeSpanStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        TimeSpan timeSpan = (TimeSpan)value;
        string result = string.Empty;

        if (timeSpan.Hours != 0)
            result += $"{timeSpan.Hours}h";

        if (timeSpan.Minutes != 0)
        {
            if (result.Length > 0)
                result += ", ";
            result += $"{timeSpan.Minutes}m";
        }

        if (timeSpan.Seconds != 0)
        {
            if (result.Length > 0)
                result += ", ";
            result += $"{timeSpan.Seconds}s";
        }

        return result.Length == 0 ? "0s" : result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}