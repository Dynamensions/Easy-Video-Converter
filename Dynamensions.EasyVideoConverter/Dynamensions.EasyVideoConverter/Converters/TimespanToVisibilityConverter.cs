using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Dynamensions.EasyVideoConverter.Converters;

public partial class TimespanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is TimeSpan ts && ts != TimeSpan.Zero)
            return Visibility.Visible;

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
