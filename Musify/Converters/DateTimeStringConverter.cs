using Microsoft.UI.Xaml.Data;
using System.Globalization;

namespace Musify.Converters;

public class DateTimeStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        ((DateTime)value).ToString("MMM d, yyyy");

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        DateTime.ParseExact((string)value, "MMM d, yyyy", CultureInfo.InvariantCulture);
}