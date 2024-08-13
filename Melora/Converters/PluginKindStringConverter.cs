using Microsoft.UI.Xaml.Data;
using Melora.Plugins.Enums;

namespace Melora.Converters;

public class PluginKindStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        (PluginKind)value switch
        {
            PluginKind.PlatformSupport => "Platform Support",
            PluginKind.Metadata => "Metadata",
            _ => "N/A"
        };

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        (string)value switch
        {
            "Platform Support" => PluginKind.PlatformSupport,
            "Metadata" => PluginKind.Metadata,
            _ => default!
        };
}