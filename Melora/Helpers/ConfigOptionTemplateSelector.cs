using Melora.Plugins.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Melora.Helpers;

public class ConfigOptionTemplateSelector : DataTemplateSelector
{
    public DataTemplate StringTemplate { get; set; } = default!;

    public DataTemplate BoolTemplate { get; set; } = default!;

    public DataTemplate IntTemplate { get; set; } = default!;
    
    public DataTemplate DoubleTemplate { get; set; } = default!;
    
    public DataTemplate SelectableTemplate { get; set; } = default!;


    public DataTemplate InvalidTemplate { get; set; } = default!;


    protected override DataTemplate SelectTemplateCore(
        object item,
        DependencyObject container)
    {
        if (item is StringOption)
            return StringTemplate;
        if (item is BoolOption)
            return BoolTemplate;
        if (item is IntOption)
            return IntTemplate;
        if (item is DoubleOption)
            return DoubleTemplate;
        if (item is SelectableOption)
            return SelectableTemplate;

        return InvalidTemplate;
    }
}