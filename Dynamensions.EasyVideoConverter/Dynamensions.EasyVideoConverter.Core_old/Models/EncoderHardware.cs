namespace Dynamensions.EasyVideoConverter.Core.Models;

public class EncoderHardware
{
    // -----------------------------
    // GPU CAPABILITIES
    // -----------------------------
    public bool SupportsNvenc { get; set; }
    public bool SupportsQsv { get; set; }
    public bool SupportsAmf { get; set; }

    // -----------------------------
    // CODEC SUPPORT
    // -----------------------------
    public bool SupportsHevc { get; set; }
    public bool SupportsAv1 { get; set; }

    // -----------------------------
    // PERFORMANCE TIERS
    // -----------------------------
    public int CpuTier { get; set; } = 3;   // 1–5
    public int GpuTier { get; set; } = 3;   // 1–5

    // -----------------------------
    // MEMORY
    // -----------------------------
    public int MemoryGB { get; set; } = 8;

    // -----------------------------
    // FLAGS
    // -----------------------------
    public bool AllowHardwareEncoding { get; set; } = true;
    public bool PreferHardwareOverCpu { get; set; } = true;

    // -----------------------------
    // OPTIONAL NAMES
    // -----------------------------
    public string? GpuName { get; set; }
    public string? CpuName { get; set; }
}
