using Microsoft.UI.Xaml.Data;
using System;

namespace Dynamensions.EasyVideoConverter.Converters;

public partial class NullToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Core.Models.Subtitles.BurnInLanguageItem languageItem)
        {
            return languageItem.Code == null ? 0.4 : 1.0;
        }

        return 1.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
