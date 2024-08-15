using Melora.Plugins.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Melora.Helpers;

public class ConfigItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate InvalidTemplate { get; set; } = default!;

    public DataTemplate StringTemplate { get; set; } = default!;

    public DataTemplate LongTemplate { get; set; } = default!;

    public DataTemplate BoolTemplate { get; set; } = default!;


    protected override DataTemplate SelectTemplateCore(
        object item,
        DependencyObject container)
    {
        if (item is not PluginConfigItem configItem)
            return InvalidTemplate;

        return Type.GetTypeCode(configItem.Value.GetType()) switch
        {
            TypeCode.String => StringTemplate,
            TypeCode.Int64 => LongTemplate,
            TypeCode.Boolean => BoolTemplate,
            _ => InvalidTemplate,
        };
    }
}