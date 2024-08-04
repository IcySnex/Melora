using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Windows.Input;
using Windows.System;

namespace Musify.Helpers;

public class Attributes : DependencyObject
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.RegisterAttached(
        "Title", typeof(string), typeof(Attributes), new(null));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.RegisterAttached(
        "Description", typeof(string), typeof(Attributes), new(null));

    public static readonly DependencyProperty ImageUrlProperty = DependencyProperty.RegisterAttached(
        "ImageUrl", typeof(string), typeof(Attributes), new(null));

    public static readonly DependencyProperty GlyphProperty = DependencyProperty.RegisterAttached(
        "Glyph", typeof(string), typeof(Attributes), new(null));

    public static readonly DependencyProperty IconPathDataProperty = DependencyProperty.RegisterAttached(
        "IconPathData", typeof(Geometry), typeof(Attributes), new(null));

    public static readonly DependencyProperty EnterKeyCommandProperty = DependencyProperty.RegisterAttached(
        "EnterKeyCommand", typeof(ICommand), typeof(Attributes), new(null, OnEnterKeyCommandChanged));


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


    public static void SetImageUrl(
        UIElement element,
        string value) =>
        element.SetValue(ImageUrlProperty, value);

    public static string GetImageUrl(
        UIElement element) =>
        (string)element.GetValue(ImageUrlProperty);


    public static void SetGlyph(
        UIElement element,
        string value) =>
        element.SetValue(GlyphProperty, value);

    public static string GetGlyph(
        UIElement element) =>
        (string)element.GetValue(GlyphProperty);


    public static void SetIconPathData(
        UIElement element,
        Geometry value) =>
        element.SetValue(IconPathDataProperty, value);

    public static Geometry GetIconPathData(
        UIElement element) =>
        (Geometry)element.GetValue(IconPathDataProperty);


    public static void SetEnterKeyCommand(
        UIElement target,
        ICommand value) =>
        target.SetValue(EnterKeyCommandProperty, value);

    public static ICommand GetEnterKeyCommand(
        UIElement target) =>
        (ICommand)target.GetValue(EnterKeyCommandProperty);

    static void OnEnterKeyCommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        ICommand command = (ICommand)e.NewValue;
        UIElement control = (UIElement)target;

        control.KeyDown += (s, args) =>
        {
            if (args.Key != VirtualKey.Enter)
                return;

            command.Execute(null);
        };
    }
}