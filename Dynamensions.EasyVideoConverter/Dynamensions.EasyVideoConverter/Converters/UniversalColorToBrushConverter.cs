using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace Dynamensions.EasyVideoConverter.Converters;

public partial class UniversalColorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string input || string.IsNullOrWhiteSpace(input))
            return new SolidColorBrush(Colors.Gray);

        input = input.Trim();

        // Named colors
        switch (input.ToLowerInvariant())
        {
            case "red": return new SolidColorBrush(Color.FromArgb(255, 244, 67, 54));
            case "green": return new SolidColorBrush(Color.FromArgb(255, 76, 175, 80));
            case "yellow": return new SolidColorBrush(Color.FromArgb(255, 255, 193, 7));
            case "purple": return new SolidColorBrush(Color.FromArgb(255, 156, 39, 176));
            case "blue": return new SolidColorBrush(Color.FromArgb(255, 33, 150, 243));
            case "orange": return new SolidColorBrush(Color.FromArgb(255, 255, 152, 0));
            case "gray": return new SolidColorBrush(Colors.Gray);
            case "white": return new SolidColorBrush(Colors.White);
            case "black": return new SolidColorBrush(Colors.Black);
            case "transparent": return new SolidColorBrush(Colors.Transparent);
        }

        // Hex (#RGB, #RRGGBB, #AARRGGBB)
        if (input.StartsWith('#'))
        {
            try { return new SolidColorBrush(ParseHexColor(input)); }
            catch { return new SolidColorBrush(Colors.Gray); }
        }

        // rgb(r,g,b)
        if (input.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            try { return new SolidColorBrush(ParseRgbColor(input)); }
            catch { return new SolidColorBrush(Colors.Gray); }
        }

        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();

    private static Color ParseHexColor(string hex)
    {
        hex = hex.Replace("#", "");

        if (hex.Length == 3)
        {
            byte r = System.Convert.ToByte(new string(hex[0], 2), 16);
            byte g = System.Convert.ToByte(new string(hex[1], 2), 16);
            byte b = System.Convert.ToByte(new string(hex[2], 2), 16);
            return Color.FromArgb(255, r, g, b);
        }

        if (hex.Length == 6)
        {
            byte r = System.Convert.ToByte(hex[..2], 16);
            byte g = System.Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = System.Convert.ToByte(hex.Substring(4, 2), 16);
            return Color.FromArgb(255, r, g, b);
        }

        if (hex.Length == 8)
        {
            byte a = System.Convert.ToByte(hex[..2], 16);
            byte r = System.Convert.ToByte(hex.Substring(2, 2), 16);
            byte g = System.Convert.ToByte(hex.Substring(4, 2), 16);
            byte b = System.Convert.ToByte(hex.Substring(6, 2), 16);
            return Color.FromArgb(a, r, g, b);
        }

        throw new FormatException("Invalid hex color");
    }

    private static Color ParseRgbColor(string rgb)
    {
        var parts = rgb.Replace("rgb(", "").Replace(")", "").Split(',');
        byte r = byte.Parse(parts[0]);
        byte g = byte.Parse(parts[1]);
        byte b = byte.Parse(parts[2]);
        return Color.FromArgb(255, r, g, b);
    }
}