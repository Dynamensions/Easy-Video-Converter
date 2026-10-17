using Dynamensions.EasyVideoConverter.Core.Extensions;

namespace Dynamensions.EasyVideoConverter.Core.Models.Profiles;

public enum DeviceType
{
    Unknown,
    StreamingBox,
    StreamingDongle,
    SmartTV,
    GameConsole,
    PC,
    Mobile,
    Tablet
}

public class ClientProfile
{
    // ---------------------------------------------------------
    // BASIC IDENTIFICATION
    // ---------------------------------------------------------
    public string Name { get; set; } = "Unknown Client";
    public string DeviceFamily { get; set; } = "Generic";
    public DeviceType DeviceType { get; set; } = DeviceType.Unknown;

    // ---------------------------------------------------------
    // DIRECT PLAY CAPABILITIES
    // ---------------------------------------------------------
    public List<string> DirectPlayVideoCodecs { get; set; } = [];
    public List<string> DirectPlayAudioCodecs { get; set; } = [];
    public List<string> DirectPlayContainers { get; set; } = [];

    // ---------------------------------------------------------
    // DIRECT STREAM CAPABILITIES
    // ---------------------------------------------------------
    public List<string> DirectStreamVideoCodecs { get; set; } = [];
    public List<string> DirectStreamAudioCodecs { get; set; } = [];
    public List<string> DirectStreamContainers { get; set; } = [];


    // ---------------------------------------------------------
    // AUDIO CAPABILITIES (NEW)
    // ---------------------------------------------------------
    // Maximum number of audio channels the device can decode.
    // Roku Ultra / Premiere+ = 8 (7.1)
    // Older Roku / Chromecast / Web browsers = 2 (stereo)
    public int MaxAudioChannels { get; set; } = 2;

    // Whether the device supports Dolby Atmos (EAC3 JOC or MAT)
    public bool SupportsAtmos { get; set; } = false;

    // Whether the device prefers downmixing multichannel audio to stereo
    public bool ForceStereoDownmix { get; set; } = false;

    public required bool RequiresAacStereoFallback { get; set; }


    // ---------------------------------------------------------
    // PREFERRED OUTPUT FORMAT
    // ---------------------------------------------------------
    public string PreferredVideoCodec { get; set; } = "h264";
    public string PreferredAudioCodec { get; set; } = "aac";
    public string PreferredContainer { get; set; } = "mp4";

    // ---------------------------------------------------------
    // RESOLUTION LIMITS
    // ---------------------------------------------------------
    public int MaxWidth { get; set; } = 1920;
    public int MaxHeight { get; set; } = 1080;

    public Resolution[] SupportedResolutions { get; set; } = [];

    // ---------------------------------------------------------
    // STRICT H.264 COMPLIANCE (Roku)
    // ---------------------------------------------------------
    public bool RequiresStrictH264Compliance { get; set; } = false;
    public string RequiredProfile { get; set; } = "high";
    public string RequiredLevel { get; set; } = "4.1";

    // ---------------------------------------------------------
    // BIT DEPTH RULES
    // ---------------------------------------------------------
    public bool Requires8Bit { get; set; } = false;

    // ---------------------------------------------------------
    // SUBTITLE CAPABILITIES
    // ---------------------------------------------------------
    public bool SupportsInternalSubtitles { get; set; } = true;
    public bool SupportsExternalSubtitles { get; set; } = true;

    public bool SupportsSrtSubtitles { get; set; } = true;
    public bool SupportsAssSubtitles { get; set; } = false;
    public bool SupportsPgsSubtitles { get; set; } = false;
    public bool SupportsMovTextSubtitles { get; set; } = true;

    public bool RequiresBurnInForUnsupportedFormats { get; set; } = true;

    // ---------------------------------------------------------
    // DIRECT PLAY LOGIC
    // ---------------------------------------------------------
    public bool CanDirectPlay(MediaInfo info)
    {
        if (!DirectPlayContainers.Contains(info.NormalizedContainer))
            return false;

        if (!DirectPlayVideoCodecs.Contains(info.VideoCodec))
            return false;

        if (!DirectPlayAudioCodecs.Contains(info.PrimaryAudioCodec))
            return false;

        // NEW: Channel count check
        if (info.AudioChannels > MaxAudioChannels)
            return false;

        if (info.Width > MaxWidth || info.Height > MaxHeight)
            return false;

        if (SupportedResolutions.Length > 0)
        {
            bool matches = SupportedResolutions.Any(r =>
                r.Width == info.Width && r.Height == info.Height);

            if (!matches)
                return false;
        }

        if (RequiresStrictH264Compliance && info.VideoCodec == "h264")
        {
            if (!string.Equals(info.H264Profile, RequiredProfile, StringComparison.OrdinalIgnoreCase))
                return false;

            if (info.H264Level == null || info.H264Level.Value.ToString("0.0") != RequiredLevel)
                return false;
        }

        if (Requires8Bit && info.BitDepth > 8)
            return false;

        return true;
    }

    // ---------------------------------------------------------
    // DIRECT STREAM LOGIC
    // ---------------------------------------------------------
    public bool CanDirectStream(MediaInfo info)
    {
        // Must have matching video codec
        bool videoOk =
            DirectStreamVideoCodecs != null &&
            DirectStreamVideoCodecs.Contains(info.VideoCodec);

        // Must have matching audio codec
        bool audioOk =
            DirectStreamAudioCodecs != null &&
            DirectStreamAudioCodecs.Contains(info.PrimaryAudioCodec);

        // Must have a container we can remux FROM
        bool containerOk =
            DirectStreamContainers != null &&
            DirectStreamContainers.Contains(info.Container);

        // All three must be true
        return videoOk && audioOk && containerOk;
    }


    public string Capabilities
    {
        get
        {
            List<string> items =
            [
                PreferredContainer,
                $"{MaxWidth}x{MaxHeight}",
                PreferredAudioCodec,
                $"{MaxAudioChannels} ch"
            ];

            return String.Join(", ", items);
        }
    }

    public string? Description { get; set; }

    public override string ToString()
    {
        return string.Join(Environment.NewLine, this.DumpProperties());
    }
}

public record Resolution(int Width, int Height);
