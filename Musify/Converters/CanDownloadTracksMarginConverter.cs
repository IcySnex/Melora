using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Musify.Converters;

public class CanDownloadTracksMarginConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        (bool)value ? new Thickness(48, 116, 48, 138) : new Thickness(48, 116, 48, 24);

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        ((Thickness)value).Bottom == 138;
}