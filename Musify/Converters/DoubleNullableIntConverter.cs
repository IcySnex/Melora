using Microsoft.UI.Xaml.Data;

namespace Musify.Converters;

public class NullableIntDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is int number ? number : double.NaN;

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is double d && double.IsNaN(d))
            return null;

        return System.Convert.ToInt32(value);
    }
}