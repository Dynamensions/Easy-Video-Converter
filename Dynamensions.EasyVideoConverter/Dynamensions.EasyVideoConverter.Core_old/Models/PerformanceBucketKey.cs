namespace Dynamensions.EasyVideoConverter.Core.Models;

public class PerformanceBucketKey
{
    public string InputCodec { get; set; } = string.Empty;
    public string OutputCodec { get; set; } = string.Empty;

    public int Width { get; set; }
    public int Height { get; set; }

    public int BitDepth { get; set; }

    // CPU / NVIDIA / QSV / AMF
    public string EncoderType { get; set; } = "CPU";

    public bool BurnIn { get; set; }

    public override int GetHashCode() => ToKeyString().GetHashCode();

    public override bool Equals(object? obj)
    {
        if (obj is PerformanceBucketKey other)
            return ToKeyString() == other.ToKeyString();

        return false;
    }

    public string ToKeyString()
    {
        return $"{Width}x{Height}_{InputCodec}_to_{OutputCodec}_{BitDepth}bit_{EncoderType}" +
               (BurnIn ? "_burnin" : "");
    }
}
