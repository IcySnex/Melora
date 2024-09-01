using Melora.Plugins.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Melora.Helpers;

public class ConfigItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate InvalidTemplate { get; set; } = default!;

    public DataTemplate StringTemplate { get; set; } = default!;

    public DataTemplate IntTemplate { get; set; } = default!;
    
    public DataTemplate DoubleTemplate { get; set; } = default!;

    public DataTemplate BoolTemplate { get; set; } = default!;


    protected override DataTemplate SelectTemplateCore(
        object item,
        DependencyObject container)
    {
        if (item is not PluginConfigItem configItem)
            return InvalidTemplate;

        return configItem.Type switch
        {
            "System.String" => StringTemplate,
            "System.Int32" => IntTemplate,
            "System.Double" => DoubleTemplate,
            "System.Boolean" => BoolTemplate,
            _ => InvalidTemplate,
        };
    }
}