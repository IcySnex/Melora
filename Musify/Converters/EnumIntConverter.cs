using Microsoft.UI.Xaml.Data;

namespace Musify.Converters;

public class EnumIntConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        (int)value;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        Enum.ToObject(targetType, (int)value == -1 ? 0 : value);
}