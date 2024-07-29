using Microsoft.UI.Xaml.Data;

namespace Musify.Converters;

public class TimeSpanLongStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        TimeSpan timeSpan = (TimeSpan)value;
        string result = string.Empty;

        if (timeSpan.Hours != 0)
            result += $"{timeSpan.Hours} hrs";

        if (timeSpan.Minutes != 0)
        {
            if (result.Length > 0)
                result += ", ";
            result += $"{timeSpan.Minutes} min";
        }

        if (timeSpan.Seconds != 0)
        {
            if (result.Length > 0)
                result += ", ";
            result += $"{timeSpan.Seconds} sec";
        }

        return result.Length == 0 ? "0 sec" : result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}