namespace Dynamensions.EasyVideoConverter.Core.Models.Profiles;

public class EncoderProfile
{
    public string Name { get; set; } = "Unknown Encoder";

    // Hardware identity (optional, but useful for UI/logs)
    public string GpuName { get; set; } = "Unknown";

    // Capability flags
    public bool SupportsNvenc { get; set; }
    public bool SupportsQsv { get; set; }
    public bool SupportsAmf { get; set; }

    // CPU fallback
    public int CpuCores { get; set; } = Environment.ProcessorCount;

    public string Description { get; set; } = string.Empty;
}

