using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace TechnoPoss.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        public static InverseBoolConverter Instance { get; } = new InverseBoolConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is bool boolValue)
            {
                if (parameter?.ToString() == "End")
                    return boolValue ? LayoutOptions.End : LayoutOptions.Start;
                else
                    return !boolValue;
            }
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            return null;
        }
    }

}