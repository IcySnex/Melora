using Microsoft.UI.Xaml.Data;

namespace Melora.Converters;

public class EnumIndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        Array.IndexOf(Enum.GetValues(value.GetType()), value);

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        Array values = Enum.GetValues(targetType);
        return values.GetValue((int)value) ?? values.GetValue(0)!;
    }
}