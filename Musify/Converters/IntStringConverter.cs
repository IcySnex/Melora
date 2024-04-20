using Microsoft.UI.Xaml.Data;

namespace Musify.Converters;

public class IntStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        ((int)value).ToString("#,0");

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        int.Parse((string)value);
}