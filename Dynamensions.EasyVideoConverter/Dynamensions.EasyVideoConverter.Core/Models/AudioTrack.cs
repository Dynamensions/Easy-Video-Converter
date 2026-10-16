namespace Dynamensions.EasyVideoConverter.Core.Models;

public class AudioTrack
{
    // ---------------------------------------------------------
    // INDEXES
    // ---------------------------------------------------------
    public int Index { get; set; }            // FFprobe index
    public int FfmpegIndex { get; set; }      // FFmpeg per-type index (0:a:0)

    // ---------------------------------------------------------
    // BASIC METADATA
    // ---------------------------------------------------------
    public string Codec { get; set; } = "unknown";
    public int? Channels { get; set; }
    public long BitRate { get; set; }

    public string Language { get; set; } = "und";
    public string? Title { get; set; }

    // ---------------------------------------------------------
    // ADVANCED AUDIO METADATA (NEW)
    // ---------------------------------------------------------

    // Sample rate (Hz)
    public int SampleRate { get; set; }

    // Channel layout (stereo, 5.1(side), 7.1)
    public string ChannelLayout { get; set; } = "unknown";

    // Bit depth (16, 24)
    public int BitDepth { get; set; }

    // ---------------------------------------------------------
    // CODEC FLAGS (NEW)
    // ---------------------------------------------------------
    public bool IsAac => Codec.StartsWith("aac", StringComparison.OrdinalIgnoreCase);
    public bool IsAacLc { get; set; }         // You already had this

    public bool IsAc3 => Codec.Equals("ac3", StringComparison.OrdinalIgnoreCase);
    public bool IsEac3 => Codec.Equals("eac3", StringComparison.OrdinalIgnoreCase);

    public bool IsDts => Codec.StartsWith("dts", StringComparison.OrdinalIgnoreCase);
    public bool IsTrueHd => Codec.Equals("truehd", StringComparison.OrdinalIgnoreCase);

    // Atmos detection (EAC3 JOC)
    public bool IsAtmos { get; set; }

    // ---------------------------------------------------------
    // CONVENIENCE FLAGS
    // ---------------------------------------------------------
    public bool IsSurround => Channels >= 6;
    public bool IsStereo => Channels == 2;
    public bool IsMono => Channels == 1;

    public bool IsMultichannel => Channels > 2;
}
