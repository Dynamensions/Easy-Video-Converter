using Dynamensions.EasyVideoConverter.Core.Models.Profiles;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace Dynamensions.EasyVideoConverter.Converters;

public partial class DeviceTypeToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not DeviceType type)
            return CreateSvg("Assets/Icons/device.svg");

        var path = type switch
        {
            DeviceType.StreamingBox => "Assets/Icons/streaming-box.svg",
            DeviceType.StreamingDongle => "Assets/Icons/dongle.svg",
            DeviceType.SmartTV => "Assets/Icons/tv.svg",
            DeviceType.GameConsole => "Assets/Icons/console.svg",
            DeviceType.PC => "Assets/Icons/pc.svg",
            DeviceType.Mobile => "Assets/Icons/mobile.svg",
            DeviceType.Tablet => "Assets/Icons/tablet.svg",
            _ => "Assets/Icons/device.svg"
        };

        return CreateSvg(path);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();

    private static SvgImageSource CreateSvg(string relativePath)
    {
        var uri = new Uri($"ms-appx:///{relativePath}");
        return new SvgImageSource(uri)
        {
            RasterizePixelWidth = 24,
            RasterizePixelHeight = 24
        };
    }
}