using Microsoft.UI.Xaml.Data;
using System;

namespace Dynamensions.EasyVideoConverter.Converters;

public partial class LongToFileSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is long bytes)
        {
            bool isNegitive = bytes < 0;
            if (isNegitive) bytes *= -1;
            // Units array
            string[] sizes = ["Bytes", "KB", "MB", "GB", "TB"];

            // If size is 0 bytes
            if (bytes == 0)
                return "0 Bytes";

            // Determine unit
            int unitIndex = (int)Math.Floor(Math.Log(bytes, 1024));
            double adjustedSize = bytes / Math.Pow(1024, unitIndex);

            var operatorValue = isNegitive ? "-" : "+";

            // Format with 2 decimal places
            return $"{operatorValue}{adjustedSize:0.##} {sizes[unitIndex]}";
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
