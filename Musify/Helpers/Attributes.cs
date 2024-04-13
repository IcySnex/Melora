using Microsoft.UI.Xaml;

namespace Musify.Helpers;

public class Attributes : DependencyObject
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.RegisterAttached(
        "Title", typeof(string), typeof(Attributes), new(null));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.RegisterAttached(
        "Description", typeof(string), typeof(Attributes), new(null));

    public static readonly DependencyProperty GlyphProperty = DependencyProperty.RegisterAttached(
        "Glyph", typeof(string), typeof(Attributes), new(null));


    public static void SetTitle(
        UIElement element,
        string value) =>
        element.SetValue(TitleProperty, value);

    public static string GetTitle(
        UIElement element) =>
        (string)element.GetValue(TitleProperty);


    public static void SetDescription(
        UIElement element,
        string value) =>
        element.SetValue(DescriptionProperty, value);

    public static string GetDescription(
        UIElement element) =>
        (string)element.GetValue(DescriptionProperty);


    public static void SetGlyph(
        UIElement element,
        string value) =>
        element.SetValue(GlyphProperty, value);

    public static string GetGlyph(
        UIElement element) =>
        (string)element.GetValue(GlyphProperty);
}