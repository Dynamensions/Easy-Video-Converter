using System.Text.Json.Serialization;

namespace Dynamensions.EasyVideoConverter.Core.Models.Ffprobe;

public class FfprobeStream
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("codec_type")]
    public string CodecType { get; set; } = string.Empty;

    [JsonPropertyName("codec_name")]
    public string CodecName { get; set; } = string.Empty;

    // ---------------------------------------------------------
    // VIDEO-SPECIFIC FIELDS
    // ---------------------------------------------------------

    // Pixel format (yuv420p, yuv420p10le, etc.)
    [JsonPropertyName("pix_fmt")]
    public string PixFmt { get; set; } = string.Empty;

    // H.264 profile (high, main, baseline)
    [JsonPropertyName("profile")]
    public string? Profile { get; set; }

    // H.264 level (41 → 4.1)
    [JsonPropertyName("level")]
    public int? Level { get; set; }

    [JsonPropertyName("color_primaries")]
    public string? ColorPrimaries { get; set; }

    [JsonPropertyName("color_space")]
    public string? ColorSpace { get; set; }

    // Color transfer (smpte2084, arib-std-b67)
    [JsonPropertyName("color_transfer")]
    public string? ColorTransfer { get; set; }

    // Display resolution
    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    // Coded resolution (macroblock padded)
    [JsonPropertyName("coded_width")]
    public int? CodedWidth { get; set; }

    [JsonPropertyName("coded_height")]
    public int? CodedHeight { get; set; }

    // Sample Aspect Ratio (e.g., "3:4", "719:960", "1:1")
    [JsonPropertyName("sample_aspect_ratio")]
    public string? SampleAspectRatio { get; set; }

    // Display Aspect Ratio (optional)
    [JsonPropertyName("display_aspect_ratio")]
    public string? DisplayAspectRatio { get; set; }

    // Frame rate (avg_frame_rate: "30000/1001")
    [JsonPropertyName("avg_frame_rate")]
    public string AvgFrameRate { get; set; } = "0/0";

    // Bitrate
    [JsonPropertyName("bit_rate")]
    public string? BitRate { get; set; }

    // Audio channels (for audio streams)
    [JsonPropertyName("channels")]
    public int Channels { get; set; }

    // ---------------------------------------------------------
    // TAGS (language, title)
    // ---------------------------------------------------------
    [JsonPropertyName("tags")]
    public FfprobeTags? Tags { get; set; }

    // ---------------------------------------------------------
    // SIDEDATA 
    // ---------------------------------------------------------
    [JsonPropertyName("side_data_list")]
    public List<FfprobeSideData>? SideDataList { get; set; }

}
