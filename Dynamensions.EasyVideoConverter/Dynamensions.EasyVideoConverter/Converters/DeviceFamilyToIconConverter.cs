using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace Dynamensions.EasyVideoConverter.Converters;

public partial class DeviceFamilyToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string deviceType)
            return CreateSvgSource("Assets/Icons/device.svg");

        deviceType = deviceType.ToLowerInvariant();

        return deviceType switch
        {
            "streamingbox" => CreateSvgSource("Assets/Icons/tv.svg"),
            "streamingdongle" => CreateSvgSource("Assets/Icons/tv.svg"),
            "smarttv" => CreateSvgSource("Assets/Icons/tv.svg"),
            "console" => CreateSvgSource("Assets/Icons/console.svg"),
            "browser" => CreateSvgSource("Assets/Icons/globe.svg"),
            "pc" => CreateSvgSource("Assets/Icons/laptop.svg"),
            _ => CreateSvgSource("Assets/Icons/device.svg")
        };
    }

    private static SvgImageSource CreateSvgSource(string path)
    {
        return new SvgImageSource(new Uri($"ms-appx:///{path}"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
