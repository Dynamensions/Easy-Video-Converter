using Microsoft.UI.Xaml.Data;
using System;

namespace Dynamensions.EasyVideoConverter.Converters;

public partial class DoubleToCurrentFpsStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not double d || double.IsNaN(d) || d <= 0)
            return "—";

        // If FPS is basically an integer, show it cleanly
        if (Math.Abs(d - Math.Round(d)) < 0.01)
            return $"{Math.Round(d)} fps";

        // Otherwise show two decimals
        return $"{d:F2} fps";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
