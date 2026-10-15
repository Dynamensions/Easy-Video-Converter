using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace Dynamensions.EasyVideoConverter.Converters;

public partial class EncoderTypeToSvgIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string encoderType)
            return CreateSvgIcon("Assets/Logos/cpu.svg");

        encoderType = encoderType.ToLowerInvariant();

        return encoderType switch
        {
            "nvidia" => CreateSvgIcon("Assets/Logos/nvidia.svg"),
            "qsv" => CreateSvgIcon("Assets/Logos/intel.svg"),
            "amf" => CreateSvgIcon("Assets/Logos/amd.svg"),
            "cpu" => CreateSvgIcon("Assets/Logos/cpu.svg"),
            _ => CreateSvgIcon("Assets/Logos/cpu.svg")
        };
    }

    private static ImageIcon CreateSvgIcon(string path)
    {
        return new ImageIcon
        {
            Source = new SvgImageSource(new Uri($"ms-appx:///{path}")),
            Width = 16,
            Height = 16
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
