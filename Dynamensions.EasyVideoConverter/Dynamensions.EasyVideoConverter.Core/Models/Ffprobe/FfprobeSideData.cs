using System.Text.Json.Serialization;

namespace Dynamensions.EasyVideoConverter.Core.Models.Ffprobe;

public class FfprobeSideData
{
    [JsonPropertyName("side_data_type")]
    public string SideDataType { get; set; } = string.Empty;

    // Optional fields (HDR10+, DV, mastering metadata)
    [JsonPropertyName("max_content")]
    public int? MaxContent { get; set; }

    [JsonPropertyName("max_average")]
    public int? MaxAverage { get; set; }

    [JsonPropertyName("rpu_present_flag")]
    public int? DolbyVisionRpuPresent { get; set; }
}
