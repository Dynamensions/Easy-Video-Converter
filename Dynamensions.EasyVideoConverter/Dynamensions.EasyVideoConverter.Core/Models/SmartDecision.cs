using CommunityToolkit.Mvvm.ComponentModel;
using Dynamensions.EasyVideoConverter.Core.Extensions;
using Dynamensions.EasyVideoConverter.Core.Models.Enums;

namespace Dynamensions.EasyVideoConverter.Core.Models;

public partial class SmartDecision : ObservableObject
{

    // ---------------------------------------------------------
    // DECISION TYPE (Mode)
    // ---------------------------------------------------------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ModeLabel))]
    [NotifyPropertyChangedFor(nameof(ModeColor))]
    [NotifyPropertyChangedFor(nameof(PlaybackQuality))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityColor))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityIcon))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityScore))]
    private bool isDirectPlay;


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ModeLabel))]
    [NotifyPropertyChangedFor(nameof(ModeColor))]
    [NotifyPropertyChangedFor(nameof(PlaybackQuality))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityColor))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityIcon))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityScore))]
    private bool isDirectStream;

    public string ModeLabel =>
        IsDirectPlay ? "Direct Play" :
        IsDirectStream ? "Direct Stream" :
        "Transcode";

    public string ModeColor =>
        IsDirectPlay ? "#2ECC71" :
        IsDirectStream ? "#F39C12" :
        "#E74C3C"; // Transcode


    // ---------------------------------------------------------
    // VIDEO ENCODE
    // ---------------------------------------------------------
    public string SelectedHardwareEncoder { get; set; } = "libx264";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EncoderSummary))]
    [NotifyPropertyChangedFor(nameof(EncoderColor))]
    private string encoderType = "CPU";

    public string EncoderSummary =>
        string.IsNullOrWhiteSpace(EncoderType)
        ? "CPU"
        : EncoderType;

    public string EncoderColor =>
        EncoderType switch
        {
            "NVIDIA" => "#76B900",
            "QSV" => "#0071C5",
            "AMF" => "#ED1C24",
            _ => "#7F8C8D" // CPU                   
        };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SarSummary))]
    [NotifyPropertyChangedFor(nameof(SarColor))]
    [NotifyPropertyChangedFor(nameof(PlaybackQuality))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityColor))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityIcon))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityScore))]
    private bool requiresSarFix;


    public string SarSummary =>
        RequiresSarFix ? "SAR fix applied" : "Square pixels";

    public string SarColor
    {
        get
        {
            if (!RequiresSarFix)
                return "Green"; // Normal SAR

            // Heavy SAR fix if scaling also required
            if (RequiresScaling)
                return "Red"; // Heavy anamorphic

            return "Yellow"; // Light anamorphic
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PlaybackQuality))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityColor))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityIcon))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityScore))]
    private bool requiresVideoEncode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VideoSummary))]
    [NotifyPropertyChangedFor(nameof(VideoColor))]
    private string selectedVideoCodec = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VideoSummary))]
    [NotifyPropertyChangedFor(nameof(VideoColor))]
    private string targetProfile = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VideoSummary))]
    [NotifyPropertyChangedFor(nameof(VideoColor))]
    private string targetLevel = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VideoSummary))]
    [NotifyPropertyChangedFor(nameof(VideoColor))]
    private int targetBitDepth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VideoSummary))]
    [NotifyPropertyChangedFor(nameof(VideoColor))]
    private bool isDolbyVision;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VideoSummary))]
    [NotifyPropertyChangedFor(nameof(VideoColor))]
    private bool isHdr10Plus;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VideoSummary))]
    [NotifyPropertyChangedFor(nameof(VideoColor))]
    private bool isHdr10;

    public VideoSummary VideoSummary
    {
        get
        {
            var codec = SelectedVideoCodec?.ToLower() ?? "unknown";
            var profile = string.IsNullOrWhiteSpace(TargetProfile) ? "" : TargetProfile;
            var level = string.IsNullOrWhiteSpace(TargetLevel) ? "" : $"L{TargetLevel}";
            var bit = $"{TargetBitDepth}-bit";

            string color = "SDR";

            if (IsDolbyVision)
                color = "Dolby Vision";
            else if (IsHdr10Plus)
                color = "HDR10+";
            else if (IsHdr10)
                color = "HDR10";
            else if (TargetBitDepth >= 10)
                color = "HDR (10‑bit)";

            return new VideoSummary
            {
                Codec = codec,
                Profile = profile,
                Level = level,
                BitDepth = bit,
                Color = color,
                Width = TargetWidth,
                Height = TargetHeight,
                IsHdr10 = IsHdr10,
                IsHdr10Plus = IsHdr10Plus,
                IsDolbyVision = IsDolbyVision
            };
        }
    }


    public string VideoColor
    {
        get
        {
            if (IsDolbyVision)
                return "#A020F0";   // Dolby Vision purple

            if (IsHdr10Plus)
                return "#FF8C00";   // HDR10+ orange

            if (IsHdr10)
                return "#DAA520";   // HDR10 gold

            if (TargetBitDepth >= 10)
                return "#FFD700";   // Generic HDR yellow

            return "#1E90FF";        // SDR blue
        }
    }



    // ---------------------------------------------------------
    // RESOLUTION / SCALING
    // ---------------------------------------------------------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScalingSummary))]
    [NotifyPropertyChangedFor(nameof(ResolutionAndScalingSummary))]
    [NotifyPropertyChangedFor(nameof(ResolutionAndScalingColor))]
    [NotifyPropertyChangedFor(nameof(PlaybackQuality))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityColor))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityIcon))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityScore))]
    private bool requiresScaling;

    public string ScalingSummary =>
        RequiresScaling ? "Scaling applied" : "No scaling";


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ResolutionAndScalingSummary))]
    [NotifyPropertyChangedFor(nameof(ResolutionAndScalingColor))]
    private int targetWidth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ResolutionAndScalingSummary))]
    [NotifyPropertyChangedFor(nameof(ResolutionAndScalingColor))]
    private int targetHeight;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ResolutionAndScalingSummary))]
    [NotifyPropertyChangedFor(nameof(ResolutionAndScalingColor))]
    private int sourceHeight;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ResolutionAndScalingSummary))]
    [NotifyPropertyChangedFor(nameof(ResolutionAndScalingColor))]
    private int sourceWidth;

    public string ResolutionAndScalingSummary
    {
        get
        {
            // No scaling → color based only on resolution
            if (!RequiresScaling)
            {
                if (TargetWidth >= 3840)
                    return "4K (No scaling)";
                if (TargetWidth >= 1920)
                    return "1080p (No scaling)";
                if (TargetWidth >= 1280)
                    return "720p (No scaling)";
                return "SD (No scaling)";
            }


            double scaleRatio = (double)TargetWidth / SourceWidth;

            string severity = scaleRatio switch
            {
                >= 1.5 => "Heavy upscale",
                > 1.0 => "Light upscale",
                <= 0.5 => "Heavy downscale",
                < 1.0 => "Light downscale",
                _ => "Scaled"
            };

            string resLabel = TargetWidth switch
            {
                >= 3840 => "4K",
                >= 1920 => "1080p",
                >= 1280 => "720p",
                _ => "SD"
            };

            return $"{resLabel} ({severity})";
        }
    }

    public string ResolutionAndScalingColor
    {
        get
        {
            // No scaling → resolution determines color
            if (!RequiresScaling)
            {
                if (TargetWidth >= 3840)
                    return "Green";   // 4K, perfect
                if (TargetWidth >= 1920)
                    return "Green";   // 1080p, perfect
                if (TargetWidth >= 1280)
                    return "Yellow";  // 720p, acceptable
                return "Red";         // SD, low quality
            }

            // Scaling occurred → severity determines color
            double scaleRatio = (double)TargetWidth / SourceWidth;

            if (scaleRatio >= 1.5)
                return "Red";        // Heavy upscale
            if (scaleRatio > 1.0)
                return "Yellow";     // Light upscale
            if (scaleRatio <= 0.5)
                return "Red";        // Heavy downscale
            if (scaleRatio < 1.0)
                return "Yellow";     // Light downscale

            return "Yellow";         // Generic scaling
        }
    }




    // ---------------------------------------------------------
    // AUDIO ENCODE
    // ---------------------------------------------------------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PlaybackQuality))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityColor))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityIcon))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityScore))]
    private bool requiresAudioEncode;

    public int TargetAudioBitrateKbps { get; set; }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AudioSummary))]
    [NotifyPropertyChangedFor(nameof(AudioColor))]
    private string selectedAudioCodec = string.Empty;


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AudioSummary))]
    [NotifyPropertyChangedFor(nameof(AudioColor))]
    private bool requiresAacStereoFallback;


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AudioSummary))]
    [NotifyPropertyChangedFor(nameof(AudioColor))]
    private int targetAudioChannels = 2;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AudioSummary))]
    [NotifyPropertyChangedFor(nameof(AudioColor))]
    private bool isAtmos;

    public bool AudioDownmixToStereo { get; set; }
    public AudioSummary AudioSummary
    {
        get
        {
            return new AudioSummary
            {
                Codec = SelectedAudioCodec?.ToLower() ?? "unknown",
                Channels = TargetAudioChannels,
                AudioBitrateKbps = TargetAudioBitrateKbps,
                IsAtmos = IsAtmos,
                RequiresAacStereoFallback = RequiresAacStereoFallback,
            };
        }
    }


    public string AudioColor =>
        TargetAudioChannels switch
        {
            2 => "#BDC3C7",
            6 => "#2980B9",
            8 => "#5B2C6F",
            _ => "#F1C40F" // Atmos or unusual layouts    
        };


    // ---------------------------------------------------------
    // CONTAINER
    // ---------------------------------------------------------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ContainerSummary))]
    [NotifyPropertyChangedFor(nameof(ContainerColor))]
    private string targetContainer = string.Empty;

    public string ContainerSummary =>
        TargetContainer?.ToLower() ?? "unknown";

    public string ContainerColor
    {
        get
        {
            string c = TargetContainer?.ToLower() ?? "unknown";

            // Direct Play containers (green)
            if (c is "mp4" or "mov" or "m4v")
                return "Green";

            // Direct Stream containers (yellow)
            if (c is "mkv" or "ts" or "avi" or "webm")
                return "Yellow";

            // Problematic containers (purple)
            if (c is "m2ts" or "blu-ray" or "bdav")
                return "Purple";

            // Transcode-forcing containers (red)
            return "Red";
        }
    }



    // ---------------------------------------------------------
    // SUBTITLES
    // ---------------------------------------------------------

    // How subtitles will be handled:
    // BurnIn = burn into video
    // Copy = soft subs (non-Roku clients)
    // None = strip all subs
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubtitleSummary))]
    [NotifyPropertyChangedFor(nameof(SubtitleColor))]
    [NotifyPropertyChangedFor(nameof(PlaybackQuality))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityColor))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityIcon))]
    [NotifyPropertyChangedFor(nameof(PlaybackQualityScore))]
    private SubtitleMode subtitleHandling;

    // The subtitle chosen for burn-in (if any)
    public SubtitleTrack? SelectedSubtitle { get; set; }

    // External subtitles to export (all preferred languages except burn-in)
    public List<SubtitleTrack> ExternalSubtitles { get; set; } = [];

    // User preferred languages (from AppSettings)
    public List<string> PreferredSubtitleLanguages { get; set; } = [];

    // Whether subtitles force transcoding (Roku always true)
    public bool SubtitlesForceTranscode { get; set; }

    // Whether burn-in is required (forced subs, PGS, ASS, MOV_TEXT, Roku)
    public bool RequiresBurnIn { get; set; }

    public string SubtitleSummary =>
        SubtitleHandling switch
        {
            SubtitleMode.Copy => "Internal",
            SubtitleMode.Extract => "External",
            SubtitleMode.BurnIn => "Burn-in",
            _ => "None"
        };

    public string SubtitleColor =>
        SubtitleHandling switch
        {
            SubtitleMode.BurnIn => "#E74C3C",
            SubtitleMode.Copy => "#3498DB",
            SubtitleMode.Extract => "#9B59B6",
            _ => "#7F8C8D"
        };


    #region "Playback Quality"

    public string PlaybackQuality
    {
        get
        {
            string display = IsDirectPlay ? "Excellent" :
                IsDirectStream ? "Good" :
                RequiresVideoEncode || RequiresScaling || RequiresSarFix ? "Poor" :
                RequiresAudioEncode || SubtitleHandling == SubtitleMode.BurnIn ? "Fair" :
                "Fair";

            var score = PlaybackQualityScore;

            return $"{display} ({score})";
        }
    }


    public string PlaybackQualityColor
    {
        get
        {
            if (IsDirectPlay)
                return "Green";

            if (IsDirectStream)
                return "Yellow";

            // Light transcode (audio or subtitle burn-in)
            if (RequiresAudioEncode ||
                SubtitleHandling == SubtitleMode.BurnIn)
                return "Orange";

            // Heavy transcode (video, scaling, SAR)
            if (RequiresVideoEncode ||
                RequiresScaling ||
                RequiresSarFix)
                return "Red";

            // Fallback
            return "Gray";
        }
    }


    public string PlaybackQualityIcon
    {
        get
        {
            if (IsDirectPlay)
                return "\uE768"; // Checkmark (best)

            if (IsDirectStream)
                return "\uE8A1"; // Sync/Remux (good)

            // Light transcode (audio or subtitles)
            if (RequiresAudioEncode ||
                SubtitleHandling == SubtitleMode.BurnIn)
                return "\uE8D0"; // Settings/Adjust (medium)

            // Heavy transcode (video, scaling, SAR)
            if (RequiresVideoEncode ||
                RequiresScaling ||
                RequiresSarFix)
                return "\uE711"; // Warning/Error (bad)

            return "\uE946"; // Info (fallback)
        }
    }

    public int PlaybackQualityScore
    {
        get
        {
            int score = 100;

            // ---------------------------------------------------------
            // Direct Stream penalty
            // ---------------------------------------------------------
            if (IsDirectStream)
                score -= 20; // small penalty

            // ---------------------------------------------------------
            // Video transcode (heavy)
            // ---------------------------------------------------------
            if (RequiresVideoEncode)
                score -= 40;

            // ---------------------------------------------------------
            // Audio transcode (light)
            // ---------------------------------------------------------
            if (RequiresAudioEncode)
                score -= 20;

            // ---------------------------------------------------------
            // Subtitle burn-in
            // ---------------------------------------------------------
            if (SubtitleHandling == SubtitleMode.BurnIn)
                score -= 10;

            // ---------------------------------------------------------
            // Scaling severity
            // ---------------------------------------------------------
            if (RequiresScaling)
            {
                double ratio = (double)TargetWidth / SourceWidth;

                if (ratio >= 1.5 || ratio <= 0.5)
                    score -= 25; // heavy scaling
                else
                    score -= 10; // light scaling
            }

            // ---------------------------------------------------------
            // SAR fix
            // ---------------------------------------------------------
            if (RequiresSarFix)
                score -= 20;

            // ---------------------------------------------------------
            // Clamp to 0–100
            // ---------------------------------------------------------
            if (score < 0) score = 0;
            if (score > 100) score = 100;

            return score;
        }
    }


    #endregion


    public string FullSummary =>
        $"{ModeLabel} | {VideoSummary.DumpPropertiesFormatted()} | {AudioSummary} | {ResolutionAndScalingSummary} | {SubtitleSummary} | {ContainerSummary}";


    // ---------------------------------------------------------
    // OPTIONAL: REASONS / DEBUGGING / UI
    // ---------------------------------------------------------
    public List<string> Reasons { get; set; } = [];
}
