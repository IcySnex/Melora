using Microsoft.UI.Xaml.Data;

namespace Musify.Converters;

public class StringNullEmptyNaConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        string result = (string)value;
        return string.IsNullOrEmpty(result) ? "N/A" : result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}