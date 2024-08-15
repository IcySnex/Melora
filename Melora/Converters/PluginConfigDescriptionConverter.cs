using Melora.Plugins.Abstract;
using Microsoft.UI.Xaml.Data;

namespace Melora.Converters;

public class PluginConfigDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        $"{string.Join(", ", ((IPluginConfig)value).Items.Select(item => item.Name).Take(3))}...";

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}