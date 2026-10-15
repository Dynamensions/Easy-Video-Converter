using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace Dynamensions.EasyVideoConverter.Converters;

public partial class LongToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is long bytes)
        {
            // Smaller → Orange
            if (bytes < 0)
                return new SolidColorBrush(Color.FromArgb(255, 0, 153, 0)); // #009900

            // Larger → Orange
            if (bytes > 0)
                return new SolidColorBrush(Color.FromArgb(255, 255, 140, 0)); // #FF8C00
        }

        // Fallback → Gray
        return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
