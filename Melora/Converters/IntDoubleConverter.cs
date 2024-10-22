﻿using Microsoft.UI.Xaml.Data;

namespace Melora.Converters;

public class IntDoubleConverter : IValueConverter
{
    public bool CanBeNull { get; set; }


    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is int number ? number : CanBeNull ? double.NaN : 0;

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is double d && double.IsNaN(d))
            return CanBeNull ? null : 0;

        return System.Convert.ToInt32(value);
    }
}