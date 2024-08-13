using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Melora.Converters;

public class InverseBoolVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        (bool)value ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        (Visibility)value == Visibility.Collapsed;
}