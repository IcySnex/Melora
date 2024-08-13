using Microsoft.UI.Xaml.Data;

namespace Melora.Converters;

public class BoolFontIconChevronConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        (bool)value ? "\xe70e" : "\xe70d";

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        (string)value == "\xe70e";
}