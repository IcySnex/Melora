using Microsoft.UI.Xaml.Data;

namespace Musify.Converters;

public class BoolDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        (bool)value ? 1.0 : 0.0;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        (int)value == 1.0;
}