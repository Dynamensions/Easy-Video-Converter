using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace Dynamensions.EasyVideoConverter.Converters
{
    public sealed class LogLevelToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, string language)
        {
            if (value is string logLevel)
            {
                Brush levelBrush = logLevel switch
                {
                    "ERR" => new SolidColorBrush(Colors.Red),
                    "WRN" => new SolidColorBrush(Colors.Orange),
                    "INF" => new SolidColorBrush(Colors.DodgerBlue),
                    "DBG" => new SolidColorBrush(Colors.Gray),
                    _ => new SolidColorBrush(Colors.Black)
                };

                return levelBrush;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotImplementedException();

    }
}
