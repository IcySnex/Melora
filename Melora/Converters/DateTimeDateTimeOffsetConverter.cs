using Microsoft.UI.Xaml.Data;

namespace Melora.Converters;

public class DateTimeDateTimeOffsetConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        new DateTimeOffset((DateTime)value);

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        ((DateTimeOffset)value).DateTime;
}