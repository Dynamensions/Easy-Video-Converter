namespace Dynamensions.EasyVideoConverter.Core.Models.Profiles;

public class EncoderProfileCollection
{
    public List<EncoderProfile> Profiles { get; } = [];

    public EncoderProfileCollection()
    {
        // Add built-in server profiles here
        Profiles.Add(EncoderProfiles.CpuOnly());
        Profiles.Add(EncoderProfiles.Nvidia());
        Profiles.Add(EncoderProfiles.IntelQsv());
        Profiles.Add(EncoderProfiles.AmdAmf());
    }

    public EncoderProfile GetByName(string name)
    {
        return Profiles.Find(p => p.Name == name)
            ?? Profiles[0]; // fallback
    }
}



public static class EncoderProfiles
{
    public static EncoderProfile CpuOnly() =>
        new()
        {
            Name = "CPU (libx264/libx265)",
            GpuName = "None",
            SupportsNvenc = false,
            SupportsQsv = false,
            SupportsAmf = false,
            CpuCores = Environment.ProcessorCount,
            Description = "Highest quality and smallest file sizes. Slowest performance. Best for archival or when GPU encoders are unavailable."
        };

    public static EncoderProfile Nvidia() =>
        new()
        {
            Name = "NVIDIA NVENC",
            GpuName = "NVIDIA GPU",
            SupportsNvenc = true,
            Description = "Extremely fast hardware encoding with excellent quality for H.264/HEVC/AV1. Larger file sizes than CPU but ideal for real‑time transcoding."
        };

    public static EncoderProfile IntelQsv() =>
        new()
        {
            Name = "Intel QuickSync",
            GpuName = "Intel iGPU",
            SupportsQsv = true,
            Description = "Balanced speed and quality. Very efficient for H.264/HEVC. Great for laptops and low‑power systems."
        };

    public static EncoderProfile AmdAmf() =>
        new()
        {
            Name = "AMD AMF",
            GpuName = "AMD GPU",
            SupportsAmf = true,
            Description = "Fast hardware encoding for H.264/HEVC. Quality varies by GPU generation. Good for gaming rigs and general use."
        };
}
