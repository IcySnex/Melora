using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Musify.Converters;

public class IntOverZeroVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        (int)value > 0 ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}