using Microsoft.UI.Xaml.Data;

namespace Melora.Converters;

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        !(bool)value;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        !(bool)value;
}
