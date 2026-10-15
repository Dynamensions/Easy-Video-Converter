using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace Dynamensions.EasyVideoConverter.Converters;

public partial class EncoderNameToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string name)
            return new SvgImageSource(new Uri("ms-appx:///Assets/Logos/cpu.svg"));

        name = name.ToLowerInvariant();

        string path = name switch
        {
            var n when n.Contains("nvenc") => "Assets/Logos/nvidia.svg",
            var n when n.Contains("nvidia") => "Assets/Logos/nvidia.svg",
            var n when n.Contains("qsv") => "Assets/Logos/intel.svg",
            var n when n.Contains("intel") => "Assets/Logos/intel.svg",
            var n when n.Contains("amf") => "Assets/Logos/amd.svg",
            var n when n.Contains("amd") => "Assets/Logos/amd.svg",
            _ => "Assets/Logos/cpu.svg"
        };

        return new SvgImageSource(new Uri($"ms-appx:///{path}"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
