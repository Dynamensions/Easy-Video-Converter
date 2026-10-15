using System.Text.Json.Serialization;

namespace Dynamensions.EasyVideoConverter.Core.Models.Ffprobe;

public class FfprobeTags
{
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}