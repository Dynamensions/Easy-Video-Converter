using Dynamensions.EasyVideoConverter.Core.Models.Enums;

namespace Dynamensions.EasyVideoConverter.Core.Models;

public class TranscodeTarget
{
    // ---------------------------------------------------------
    // VIDEO TARGET
    // ---------------------------------------------------------
    public string VideoCodec { get; set; } = string.Empty;      // e.g. "h264"
    public string VideoProfile { get; set; } = string.Empty;    // e.g. "high"
    public string VideoLevel { get; set; } = string.Empty;      // e.g. "4.1"
    public int BitDepth { get; set; }                           // 8 or 10

    // ---------------------------------------------------------
    // RESOLUTION TARGET
    // ---------------------------------------------------------
    public int Width { get; set; }                              // e.g. 1920
    public int Height { get; set; }                             // e.g. 1080

    // ---------------------------------------------------------
    // AUDIO TARGET
    // ---------------------------------------------------------
    public string AudioCodec { get; set; } = string.Empty;      // e.g. "aac"
    public bool AudioIsAacLc { get; set; }                      // true if LC required

    // ---------------------------------------------------------
    // CONTAINER TARGET
    // ---------------------------------------------------------
    public string Container { get; set; } = string.Empty;       // e.g. "mp4"

    // ---------------------------------------------------------
    // SUBTITLE TARGET
    // ---------------------------------------------------------
    public SubtitleMode SubtitleMode { get; set; }              // Burn / Copy / None
}
