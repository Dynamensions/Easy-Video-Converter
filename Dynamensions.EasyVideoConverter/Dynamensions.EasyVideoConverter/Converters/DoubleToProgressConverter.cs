using Microsoft.UI.Xaml.Data;
using System;

namespace Dynamensions.EasyVideoConverter.Converters;

public partial class DoubleToProgressConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {

        if (value is not double d || double.IsNaN(d) || d <= 0)
            return "—";

        d *= 0.01;

        // percent comes in as a double above 1.
        // divide it by 100 so progrewss displays cleanly.
        return (d).ToString("P2");

    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
