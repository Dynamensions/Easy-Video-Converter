using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace Dynamensions.EasyVideoConverter.Converters;

public partial class EncoderTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string encoderType)
            return new SolidColorBrush(Colors.Gray);

        return encoderType.ToLowerInvariant() switch
        {
            "nvidia" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 180, 0)),      // green
            "qsv" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 120, 255)),    // intel blue
            "amf" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 80, 0)),     // amd orange
            "cpu" => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 180, 180, 180)),  // gray
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
