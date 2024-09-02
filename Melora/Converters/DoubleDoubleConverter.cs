using Microsoft.UI.Xaml.Data;

namespace Melora.Converters;

public class DoubleDoubleConverter : IValueConverter
{
    public bool CanBeNull { get; set; }


    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is double number ? number : 0;

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is double d && double.IsNaN(d))
            return CanBeNull ? null : 0.0;

        return value;
    }
}