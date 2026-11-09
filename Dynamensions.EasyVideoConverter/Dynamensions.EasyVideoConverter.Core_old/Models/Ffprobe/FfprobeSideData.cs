using System.Text.Json.Serialization;

namespace Dynamensions.EasyVideoConverter.Core.Models.Ffprobe;

public class FfprobeSideData
{
    [JsonPropertyName("side_data_type")]
    public string SideDataType { get; set; }

    [JsonPropertyName("dv_version_major")]
    public int DvVersionMajor { get; set; }

    [JsonPropertyName("dv_version_minor")]
    public int DvVersionMinor { get; set; }

    [JsonPropertyName("dv_profile")]
    public int DvProfile { get; set; }

    [JsonPropertyName("dv_level")]
    public int DvLevel { get; set; }

    [JsonPropertyName("rpu_present_flag")]
    public int RpuPresentFlag { get; set; }

    [JsonPropertyName("el_present_flag")]
    public int ElPresentFlag { get; set; }

    [JsonPropertyName("bl_present_flag")]
    public int BlPresentFlag { get; set; }

    [JsonPropertyName("dv_bl_signal_compatibility_id")]
    public int DvBlSignalCompatibilityId { get; set; }

    [JsonPropertyName("dv_md_compression")]
    public string DvMdCompression { get; set; }
}
