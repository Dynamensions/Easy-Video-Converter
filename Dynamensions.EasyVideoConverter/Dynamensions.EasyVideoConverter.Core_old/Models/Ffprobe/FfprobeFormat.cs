namespace Dynamensions.EasyVideoConverter.Core.Models.Ffprobe;

using System.Text.Json.Serialization;

public class FfprobeFormat
{
    // -----------------------------
    // BASIC FORMAT INFO
    // -----------------------------
    [JsonPropertyName("duration")]
    public string? Duration { get; set; }

    [JsonPropertyName("bit_rate")]
    public string? BitRate { get; set; }

    [JsonPropertyName("format_name")]
    public string? FormatName { get; set; }
}
