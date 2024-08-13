using Microsoft.UI.Xaml.Data;

namespace Melora.Converters;

public class LongDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is long number ? number : double.NaN;

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is double d && double.IsNaN(d))
            return null;

        return System.Convert.ToInt64(value);
    }
}