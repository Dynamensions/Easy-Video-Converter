namespace Dynamensions.EasyVideoConverter.Core.Models;

public class MediaInfo
{
    // ---------------------------------------------------------
    // BASIC METADATA
    // ---------------------------------------------------------
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
    public long BitRate { get; set; } = 0;

    public double BitRateMbps => BitRate / 1_000_000.0;

    // ---------------------------------------------------------
    // CONTAINER
    // ---------------------------------------------------------
    public string Container { get; set; } = "unknown";

    private static readonly Dictionary<string, string> ContainerMap = new()
    {
        { "matroska", "mkv" },
        { "webm", "webm" },
        { "mov,mp4,m4a,3gp,3g2,mj2", "mp4" },
        { "avi", "avi" },
        { "mpegts", "ts" },
        { "mpeg", "mpeg" },
        { "ogg", "ogg" },
        { "asf", "wmv" },
        { "flv", "flv" }
    };

    public string NormalizedContainer =>
        ContainerMap.TryGetValue(Container, out var normalized)
        ? normalized
        : Container;

    // ---------------------------------------------------------
    // VIDEO
    // ---------------------------------------------------------
    public string VideoCodec { get; set; } = "unknown";

    public int Width { get; set; }
    public int Height { get; set; }

    public int CodedWidth { get; set; }
    public int CodedHeight { get; set; }

    public double FrameRate { get; set; }

    public string PixelFormat { get; set; } = "unknown";

    public string? H264Profile { get; set; }
    public double? H264Level { get; set; }

    public bool IsHdr { get; set; }
    public bool Is10Bit { get; set; }
    public bool IsHdr10 { get; set; }
    public bool IsHdr10Plus { get; set; }
    public bool IsDolbyVision { get; set; }


    public int BitDepth { get; set; } = 8;

    // ---------------------------------------------------------
    // ANAMORPHIC / PAR / SAR
    // ---------------------------------------------------------
    public double PixelAspectRatio { get; set; } = 1.0;

    public int SarNum { get; set; } = 1;
    public int SarDen { get; set; } = 1;

    public bool IsAnamorphic => PixelAspectRatio != 1.0;

    // ---------------------------------------------------------
    // AUDIO TRACKS
    // ---------------------------------------------------------
    public List<AudioTrack> AudioTracks { get; set; } = [];

    public AudioTrack? PrimaryAudio { get; set; }

    public string PrimaryAudioCodec => PrimaryAudio?.Codec ?? "unknown";

    // NEW: Channel count (2, 6, 8)
    public int AudioChannels => PrimaryAudio?.Channels ?? 0;

    // NEW: Channel layout (stereo, 5.1, 7.1)
    public string AudioChannelLayout => PrimaryAudio?.ChannelLayout ?? "unknown";

    // NEW: Sample rate (44100, 48000)
    public int AudioSampleRate => PrimaryAudio?.SampleRate ?? 0;

    // NEW: Audio bitrate
    public long AudioBitrate => PrimaryAudio?.BitRate ?? 0;

    // NEW: Atmos detection (EAC3 JOC)
    public bool IsAtmos => PrimaryAudio?.IsAtmos ?? false;

    // NEW: DTS detection
    public bool IsDts => PrimaryAudio?.Codec?.StartsWith("dts", StringComparison.OrdinalIgnoreCase) ?? false;

    // NEW: TrueHD detection
    public bool IsTrueHd => PrimaryAudio?.Codec?.Equals("truehd", StringComparison.OrdinalIgnoreCase) ?? false;

    // ---------------------------------------------------------
    // SUBTITLE TRACKS
    // ---------------------------------------------------------
    public List<SubtitleTrack> Subtitles { get; set; } = [];

    public bool HasSubtitles { get; set; }

    public bool HasSrtSubtitles { get; set; }
    public bool HasAssSubtitles { get; set; }
    public bool HasPgsSubtitles { get; set; }
    public bool HasMovTextSubtitles { get; set; }

    // ---------------------------------------------------------
    // CONVENIENCE FLAGS
    // ---------------------------------------------------------
    public bool HasMultipleAudio => AudioTracks.Count > 1;
    public bool HasMultipleSubtitles => Subtitles.Count > 1;

    // ---------------------------------------------------------
    // DEBUG SUMMARY
    // ---------------------------------------------------------
    public override string ToString()
    {
        return
            $"Container: {Container}\n" +
            $"Video: {VideoCodec} {Width}x{Height} @ {FrameRate}fps\n" +
            $"Coded: {CodedWidth}x{CodedHeight}\n" +
            $"PixelFormat: {PixelFormat}\n" +
            $"Profile/Level: {H264Profile}@{H264Level}\n" +
            $"PAR: {PixelAspectRatio} (SAR {SarNum}:{SarDen})\n" +
            $"Anamorphic: {IsAnamorphic}\n" +
            $"Audio Tracks: {AudioTracks.Count} (Primary: {PrimaryAudioCodec})\n" +
            $"Channels: {AudioChannels} ({AudioChannelLayout})\n" +
            $"SampleRate: {AudioSampleRate} Hz\n" +
            $"AudioBitrate: {AudioBitrate} bps\n" +
            $"Atmos: {IsAtmos}\n" +
            $"DTS: {IsDts}\n" +
            $"TrueHD: {IsTrueHd}\n" +
            $"Subtitles: {Subtitles.Count}\n" +
            $"Bitrate: {BitRate} ({BitRateMbps:F2} Mbps)\n" +
            $"Duration: {Duration}";
    }
}
