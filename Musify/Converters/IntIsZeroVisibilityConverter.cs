using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Musify.Converters;

public class IntIsZeroVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        (int)value == 0 ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}