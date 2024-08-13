using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace Musify.Converters;

public class AdaptiveHeightValueConverter : IValueConverter
{
    public static Thickness GetItemMargin(
        GridView view,
        Thickness fallback = default)
    {
        Setter? setter = view.ItemContainerStyle?.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == FrameworkElement.MarginProperty);
        if (setter is not null)
            return (Thickness)setter.Value;

        if (view.Items.Count <= 0)
            return fallback; // Use the default thickness for a GridViewItem

        GridViewItem container = (GridViewItem)view.ContainerFromIndex(0);
        if (container is null)
            return fallback; // Use the default thickness for a GridViewItem

        return container.Margin;
    }


    Thickness thickness = new(0, 0, 4, 4);

    public Thickness DefaultItemMargin
    {
        get => thickness;
        set => thickness = value;
    }


    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is null)
            return double.NaN;

        GridView gridView = (GridView)parameter;
        if (gridView is null)
            return value;

        _ = double.TryParse(value.ToString(), out double height);

        Thickness padding = gridView.Padding;
        Thickness margin = GetItemMargin(gridView, DefaultItemMargin);

        height = height + margin.Top + margin.Bottom + padding.Top + padding.Bottom;
        return height;

    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}