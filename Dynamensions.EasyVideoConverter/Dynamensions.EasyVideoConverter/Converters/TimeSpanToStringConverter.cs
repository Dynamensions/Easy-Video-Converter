using Microsoft.UI.Xaml.Data;
using System;

namespace Dynamensions.EasyVideoConverter.Converters;

public partial class TimeSpanToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not TimeSpan ts)
            return "—";

        if (ts <= TimeSpan.Zero)
            return "—";

        // Format based on duration length
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h {ts.Minutes:D2}m";

        if (ts.TotalMinutes >= 1)
            return $"{ts.Minutes}m {ts.Seconds:D2}s";

        return $"{ts.Seconds}s";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
