using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Models.Enums;
using Dynamensions.EasyVideoConverter.Core.Models.Profiles;
using Dynamensions.EasyVideoConverter.Core.Services;
using Dynamensions.EasyVideoConverter.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Dynamensions.EasyVideoConverter.Services;

public class SmartDecisionService(ILogger<SmartDecisionService> logger, ISettingsService settingsService) : ISmartDecisionService
{
    public SmartDecision Decide(
        EncoderProfile encoder,
        ClientProfile client,
        MediaInfo info,
        SmartDecision? d = null)
    {
        if (encoder == null || client == null) return d ?? new();

        d ??= new();
        d.Reasons.Clear();

        // set required fields
        d.SourceHeight = info.Height;
        d.SourceWidth = info.Width;


        // ---------------------------------------------------------
        // USER INTENT
        // ---------------------------------------------------------
        bool userWantsBurnIn =
            !string.IsNullOrWhiteSpace(settingsService.Settings.PreferredBurninLanguage);

        var preferredLanguages = settingsService.Settings.PreferredSubtitleLanguages.ToList();

        // ---------------------------------------------------------
        // SUBTITLE SELECTION
        // ---------------------------------------------------------
        var subtitleSelection =
            SubtitleSelector.Select(info, preferredLanguages, settingsService.Settings.PreferredBurninLanguage);

        d.SelectedSubtitle = subtitleSelection.BurnIn;
        d.ExternalSubtitles = subtitleSelection.External;
        d.PreferredSubtitleLanguages = preferredLanguages;

        // log
        if (d.SelectedSubtitle != null)
        {
            SmartDecisionLog.SubtitlesImageBasedBurnIn(logger);
        }

        if (d.ExternalSubtitles != null && d.ExternalSubtitles.Count > 0)
        {
            SmartDecisionLog.SubtitlesExternalExtract(logger);
        }



        // ---------------------------------------------------------
        // CLIENT SUBTITLE CAPABILITY
        // ---------------------------------------------------------
        bool clientSupportsInternalSubs = client.SupportsInternalSubtitles;
        bool clientSupportsSrt = client.SupportsSrtSubtitles;
        bool clientSupportsExternalSubs = client.SupportsExternalSubtitles;

        // ---------------------------------------------------------
        // SUBTITLE HANDLING DECISION (CORRECTED)
        // ---------------------------------------------------------

        var hasAnySubtitle =
            d.SelectedSubtitle != null ||
            (d.ExternalSubtitles != null && d.ExternalSubtitles.Count != 0);

        if (!hasAnySubtitle)
        {
            d.SubtitleHandling = SubtitleMode.None;
            d.RequiresBurnIn = false;
            d.SubtitlesForceTranscode = false;
            d.Reasons.Add("No subtitles selected.");
        }
        else
        {
            // User explicitly requested burn-in
            if (userWantsBurnIn && d.SelectedSubtitle != null)
            {
                d.SubtitleHandling = SubtitleMode.BurnIn;
                d.RequiresBurnIn = true;
                d.SubtitlesForceTranscode = true;

                d.Reasons.Add($"User requested burn-in language: {settingsService.Settings.PreferredBurninLanguage}");
            }
            else
            {
                // Image-based subs (PGS/VobSub) → burn-in
                if (d.SelectedSubtitle != null &&
                    (d.SelectedSubtitle.Codec is "hdmv_pgs_subtitle" or "dvd_subtitle"))
                {
                    d.SubtitleHandling = SubtitleMode.BurnIn;
                    d.RequiresBurnIn = true;
                    d.SubtitlesForceTranscode = true;

                    d.Reasons.Add("Image-based subtitles (PGS/VobSub) detected → burn-in required.");
                    SmartDecisionLog.SubtitlesImageBasedBurnIn(logger);
                }
                else if (clientSupportsInternalSubs && d.SelectedSubtitle != null)
                {
                    // Client can handle internal subs → copy
                    d.SubtitleHandling = SubtitleMode.Copy;
                    d.RequiresBurnIn = false;
                    d.SubtitlesForceTranscode = false;

                    d.Reasons.Add("Client supports internal subtitles → copying.");
                    SmartDecisionLog.SubtitlesInternalCopy(logger);
                }
                else if (clientSupportsExternalSubs &&
                         d.ExternalSubtitles != null &&
                         d.ExternalSubtitles.Count != 0)
                {
                    // Client prefers/accepts external subs → extract
                    d.SubtitleHandling = SubtitleMode.Extract;
                    d.RequiresBurnIn = false;
                    d.SubtitlesForceTranscode = false;

                    d.Reasons.Add("Client does not support internal subtitles → external subtitles will be used.");
                    SmartDecisionLog.SubtitlesExternalExtract(logger);
                }
                else
                {
                    // Last resort → burn-in
                    d.SubtitleHandling = SubtitleMode.BurnIn;
                    d.RequiresBurnIn = true;
                    d.SubtitlesForceTranscode = true;

                    d.Reasons.Add("Client does not support internal/external subtitles → burn-in required.");
                    SmartDecisionLog.RokuSubtitleBurnIn(logger);
                }
            }
        }

        // ---------------------------------------------------------
        // ROKU SPECIAL CASE (CORRECTED)
        // ---------------------------------------------------------
        if (client.Name.Contains("Roku", StringComparison.OrdinalIgnoreCase))
        {
            if (d.SelectedSubtitle != null && !d.SelectedSubtitle.IsSrt)
            {
                // Roku cannot handle non-SRT internal subs → burn-in
                d.SubtitleHandling = SubtitleMode.BurnIn;
                d.RequiresBurnIn = true;
                d.SubtitlesForceTranscode = true;

                d.Reasons.Add("Roku does not support this subtitle format → burn-in required.");
                SmartDecisionLog.RokuSubtitleBurnIn(logger);
            }
            else if (clientSupportsExternalSubs &&
                     d.ExternalSubtitles != null &&
                     d.ExternalSubtitles.Count != 0)
            {
                // Roku supports external SRT → extract
                d.SubtitleHandling = SubtitleMode.Extract;
                d.RequiresBurnIn = false;
                d.SubtitlesForceTranscode = false;

                d.Reasons.Add("Roku supports external SRT → external subtitles will be used.");
                SmartDecisionLog.RokuExternalSrt(logger);
            }
            else
            {
                d.Reasons.Add("Roku subtitle capabilities checked → no override required.");
            }
        }

        // ---------------------------------------------------------
        // ANAMORPHIC DETECTION
        // ---------------------------------------------------------
        d.RequiresSarFix = info.PixelAspectRatio != 1.0;
        if (d.RequiresSarFix)
        {
            d.Reasons.Add($"Anamorphic source detected (PAR={info.PixelAspectRatio}). Square pixel correction required.");
            SmartDecisionLog.AnamorphicDetected(logger);
        }

        // ---------------------------------------------------------
        // AUDIO DECISION LOGIC (NEW)
        // ---------------------------------------------------------
        bool codecSupported = client.DirectPlayAudioCodecs.Contains(info.PrimaryAudioCodec);
        bool channelsSupported = info.AudioChannels <= client.MaxAudioChannels;

        bool isAtmos = info.IsAtmos;
        bool isDts = info.IsDts;
        bool isTrueHd = info.IsTrueHd;


        // DTS / TrueHD → always transcode
        if (isDts || isTrueHd)
        {
            d.RequiresAudioEncode = true;
            d.Reasons.Add($"Audio codec {info.PrimaryAudioCodec} is unsupported → transcoding required.");
            SmartDecisionLog.AudioDtsTrueHdTranscode(logger);
        }
        else
        {
            d.RequiresAudioEncode = !(codecSupported && channelsSupported);
        }

        if (!channelsSupported)
        {
            d.Reasons.Add($"Audio channel count {info.AudioChannels} exceeds client limit {client.MaxAudioChannels}.");
            SmartDecisionLog.AudioChannelsExceeded(logger);
        }

        // Atmos passthrough
        if (isAtmos)
        {
            if (client.SupportsAtmos)
            {
                d.IsAtmos = true;
                d.Reasons.Add("Atmos detected → passthrough allowed.");
                SmartDecisionLog.AudioAtmosPassthrough(logger);
            }
            else
            {
                d.IsAtmos = false;
                d.RequiresAudioEncode = true;
                d.Reasons.Add("Atmos detected but client does not support Atmos → transcoding required.");
                SmartDecisionLog.AudioAtmosTranscode(logger);
            }
        }

        // ---------------------------------------------------------
        // ROKU AUDIO RULES (AAC fallback + Dolby multichannel)
        // ---------------------------------------------------------

        // Multichannel?
        bool isMultichannel = info.AudioChannels > 2;

        // Roku-style devices require:
        // 1. Multichannel Dolby (EAC3)
        // 2. AAC stereo fallback
        bool requiresAacFallback = client.RequiresAacStereoFallback && isMultichannel;

        // Decide primary output codec
        if (isMultichannel)
        {
            // Multichannel → Dolby Digital Plus (EAC3)
            d.SelectedAudioCodec = "eac3";
            d.TargetAudioChannels = Math.Min(info.AudioChannels, client.MaxAudioChannels);
            d.TargetAudioBitrateKbps = 768;

            d.RequiresAacStereoFallback = requiresAacFallback;

            SmartDecisionLog.AudioMultichannelEac3(logger);

            if (requiresAacFallback)
            {
                d.Reasons.Add("Client requires AAC stereo fallback for multichannel audio.");
                SmartDecisionLog.AudioAacFallback(logger);
            }
        }
        else
        {
            // Stereo → AAC
            d.SelectedAudioCodec = "aac";
            d.TargetAudioChannels = 2;
            d.TargetAudioBitrateKbps = 128;
            SmartDecisionLog.AudioStereoAac(logger);
        }



        // ---------------------------------------------------------
        // DIRECT PLAY
        // ---------------------------------------------------------
        if (client.CanDirectPlay(info) && !d.SubtitlesForceTranscode && !d.RequiresAudioEncode)
        {
            d.IsDirectPlay = true;
            d.IsDirectStream = false;
            d.RequiresVideoEncode = false;
            d.RequiresAudioEncode = false;

            d.SelectedVideoCodec = info.VideoCodec;
            d.SelectedAudioCodec = info.PrimaryAudioCodec;
            d.TargetContainer = info.Container;

            d.SelectedHardwareEncoder = d.SelectedVideoCodec;
            d.Reasons.Add("Direct Play allowed.");
            SmartDecisionLog.DirectPlayAllowed(logger);
            return d;
        }

        // ---------------------------------------------------------
        // DIRECT STREAM
        // ---------------------------------------------------------
        if (client.CanDirectStream(info) && !d.SubtitlesForceTranscode)
        {
            d.IsDirectPlay = false;
            d.IsDirectStream = true;
            d.RequiresVideoEncode = false;

            d.RequiresAudioEncode = !codecSupported || !channelsSupported;

            if (!d.RequiresAudioEncode)
            {
                d.SelectedAudioCodec = info.PrimaryAudioCodec;
                SmartDecisionLog.DirectStreamAudioPassthrough(logger);
            }
            else
            {
                d.SelectedAudioCodec = ChooseAudioCodec(client, info);
                d.Reasons.Add($"Audio transcoding required → selected {d.SelectedAudioCodec}.");
                SmartDecisionLog.DirectStreamAudioTranscode(logger);
            }

            // Decide target audio channels
            if (d.AudioDownmixToStereo)
            {
                d.TargetAudioChannels = 2;
                d.Reasons.Add("Target audio channels set to 2 (downmix).");
                SmartDecisionLog.DirectStreamDownmixStereo(logger);
            }
            else
            {
                d.TargetAudioChannels = Math.Min(info.AudioChannels, client.MaxAudioChannels);
                d.Reasons.Add($"Target audio channels set to {d.TargetAudioChannels}.");
                SmartDecisionLog.DirectStreamTargetChannels(logger);
            }

            d.SelectedVideoCodec = info.VideoCodec;
            d.TargetContainer = client.PreferredContainer;

            d.SelectedHardwareEncoder = d.SelectedVideoCodec;
            d.Reasons.Add("Direct Stream allowed.");
            SmartDecisionLog.DirectStreamAllowed(logger);
            return d;
        }

        // ---------------------------------------------------------
        // FULL TRANSCODE
        // ---------------------------------------------------------
        d.IsDirectPlay = false;
        d.IsDirectStream = false;
        d.RequiresVideoEncode = true;

        d.Reasons.Add("Full transcode required.");
        SmartDecisionLog.FullTranscodeRequired(logger);

        // Audio
        d.RequiresAudioEncode = true;
        d.SelectedAudioCodec = ChooseAudioCodec(client, info);

        // Apply client downmix rule
        d.AudioDownmixToStereo = client.ForceStereoDownmix;

        // Decide target audio channels
        if (d.AudioDownmixToStereo)
        {
            d.TargetAudioChannels = 2;
            d.Reasons.Add("Target audio channels set to 2 (forced stereo downmix).");
            SmartDecisionLog.TranscodeDownmixStereo(logger);
        }
        else
        {
            d.TargetAudioChannels = Math.Min(info.AudioChannels, client.MaxAudioChannels);
            d.Reasons.Add($"Target audio channels set to {d.TargetAudioChannels}.");
            SmartDecisionLog.TranscodeTargetChannels(logger);
        }


        // Clamp audo bitrate to safe levels.
        ClampBitRate(client, info, d);


        // Video output format
        d.SelectedVideoCodec = client.PreferredVideoCodec;
        d.SelectedHardwareEncoder = ChooseHardwareEncoder(encoder, d);

        if (encoder.SupportsNvenc)
            d.EncoderType = "NVIDIA";
        else if (encoder.SupportsQsv)
            d.EncoderType = "QSV";
        else if (encoder.SupportsAmf)
            d.EncoderType = "AMF";
        else
            d.EncoderType = "CPU";

        d.Reasons.Add($"Selected encoder: {d.SelectedHardwareEncoder} ({d.EncoderType}).");
        SmartDecisionLog.TranscodeEncoderSelected(logger);

        // Strict H.264 compliance
        if (client.RequiresStrictH264Compliance && d.SelectedVideoCodec == "h264")
        {
            d.TargetProfile = client.RequiredProfile;
            d.TargetLevel = client.RequiredLevel;
            d.Reasons.Add($"Strict H.264 compliance required → profile={d.TargetProfile}, level={d.TargetLevel}.");
            SmartDecisionLog.StrictH264Compliance(logger);
        }

        d.TargetBitDepth = client.Requires8Bit ? 8 : info.BitDepth;
        d.TargetContainer = client.PreferredContainer;

        // ---------------------------------------------------------
        // SCALING
        // ---------------------------------------------------------
        d.RequiresScaling =
            info.Width > client.MaxWidth ||
            info.Height > client.MaxHeight;


        d.TargetWidth = client.MaxWidth;
        d.TargetHeight = client.MaxHeight;

        if (d.RequiresScaling)
        {
            d.Reasons.Add($"Scaling required → initial target {d.TargetWidth}x{d.TargetHeight}.");
            SmartDecisionLog.ScalingRequired(logger);
        }

        // ---------------------------------------------------------
        // STRICT RESOLUTION ENFORCEMENT
        // ---------------------------------------------------------
        if (client.SupportedResolutions.Length > 0)
        {
            bool matchesSupported = client.SupportedResolutions.Any(r =>
                r.Width == info.Width && r.Height == info.Height);

            if (!matchesSupported)
            {
                d.RequiresScaling = true;

                var target = client.SupportedResolutions
                    .OrderBy(r => Math.Abs(r.Height - info.Height))
                    .First();

                d.TargetWidth = target.Width;
                d.TargetHeight = target.Height;

                d.Reasons.Add($"Client requires exact resolution {target.Width}x{target.Height}.");
                SmartDecisionLog.StrictResolutionEnforced(logger);
            }
        }

        ComputeOutputHdrFlags(info, d);

        LogReasons(d);

        return d;
    }

    // ---------------------------------------------------------
    // AUDIO CODEC SELECTION (NEW)
    // ---------------------------------------------------------
    private static string ChooseAudioCodec(ClientProfile client, MediaInfo info)
    {
        int ch = info.AudioChannels;

        // Atmos passthrough
        if (info.IsAtmos && client.SupportsAtmos)
            return info.PrimaryAudioCodec;

        // Multichannel
        if (ch > 2)
        {
            if (client.DirectPlayAudioCodecs.Contains("eac3"))
                return "eac3";

            if (client.DirectPlayAudioCodecs.Contains("ac3"))
                return "ac3";

            if (client.DirectPlayAudioCodecs.Contains("aac"))
                return "aac";
        }

        // Stereo fallback
        return client.PreferredAudioCodec;
    }

    private static void ClampBitRate(ClientProfile client, MediaInfo info, SmartDecision d)
    {
        int sourceKbps = (int)(info.AudioBitrate / 1000);

        // ---------------------------------------------------------
        // AUDIO BITRATE CLAMP (Roku-safe)
        // ---------------------------------------------------------
        if (d.SelectedAudioCodec == "aac")
        {
            // Roku supports AAC only up to 256 kbps
            if (sourceKbps > 256)
                d.TargetAudioBitrateKbps = 192;   // safe fallback
            else
                d.TargetAudioBitrateKbps = sourceKbps;

            d.Reasons.Add($"AAC bitrate clamped to {d.TargetAudioBitrateKbps} Kbps for Roku compatibility.");
        }
        else if (d.SelectedAudioCodec == "ac3")
        {
            // AC3 max is 640 kbps
            d.TargetAudioBitrateKbps = Math.Min(sourceKbps, 640);
            d.Reasons.Add($"AC3 bitrate capped at {d.TargetAudioBitrateKbps} Kbps.");
        }
        else if (d.SelectedAudioCodec == "eac3")
        {
            // EAC3 max is 768 kbps
            d.TargetAudioBitrateKbps = Math.Min(sourceKbps, 768);
            d.Reasons.Add($"EAC3 bitrate capped at {d.TargetAudioBitrateKbps} Kbps.");
        }
    }


    // ---------------------------------------------------------
    // VIDEO ENCODER SELECTION
    // ---------------------------------------------------------
    private static string ChooseHardwareEncoder(EncoderProfile encoder, SmartDecision d)
    {
        if (d.SelectedVideoCodec == "h264")
        {
            if (encoder.SupportsQsv) return "h264_qsv";
            if (encoder.SupportsNvenc) return "h264_nvenc";
            if (encoder.SupportsAmf) return "h264_amf";
            return "h264"; // CPU fallback (native FFmpeg encoder)
        }

        if (d.SelectedVideoCodec == "hevc")
        {
            if (encoder.SupportsQsv) return "hevc_qsv";
            if (encoder.SupportsNvenc) return "hevc_nvenc";
            if (encoder.SupportsAmf) return "hevc_amf";
            return "hevc"; // CPU fallback (native FFmpeg encoder)
        }

        if (d.SelectedVideoCodec == "av1")
        {
            if (encoder.SupportsQsv) return "av1_qsv";
            if (encoder.SupportsNvenc) return "av1_nvenc";
            if (encoder.SupportsAmf) return "av1_amf";
            return "libaom-av1"; // CPU fallback
        }

        return d.SelectedVideoCodec;
    }


    private void ComputeOutputHdrFlags(MediaInfo info, SmartDecision d)
    {
        // H.264 cannot carry HDR
        if (d.SelectedVideoCodec == "h264")
        {
            d.IsDolbyVision = false;
            d.IsHdr10Plus = false;
            d.IsHdr10 = false;
            d.TargetBitDepth = 8;

            SmartDecisionLog.HdrH264Disabled(logger);
            return;
        }

        // HEVC Main (8-bit) cannot carry HDR
        if (d.SelectedVideoCodec == "hevc" && d.TargetBitDepth == 8)
        {
            d.IsDolbyVision = false;
            d.IsHdr10Plus = false;
            d.IsHdr10 = false;

            SmartDecisionLog.HdrHevc8BitDisabled(logger);
            return;
        }

        // HEVC Main10 (10-bit) can carry HDR
        if (d.SelectedVideoCodec == "hevc" && d.TargetBitDepth >= 10)
        {
            if (info.IsDolbyVision)
            {
                d.IsDolbyVision = true;
                SmartDecisionLog.HdrDolbyVision(logger);
                return;
            }

            if (info.IsHdr10Plus)
            {
                d.IsHdr10Plus = true;
                SmartDecisionLog.Hdr10Plus(logger);
                return;
            }

            if (info.IsHdr10)
            {
                d.IsHdr10 = true;
                SmartDecisionLog.Hdr10(logger);
                return;
            }

            // SDR 10-bit fallback
            d.IsDolbyVision = false;
            d.IsHdr10Plus = false;
            d.IsHdr10 = false;

            SmartDecisionLog.HdrSdrFallback(logger);
            return;
        }
    }

    // ---------------------------------------------------------
    // LOG ALL REASONS
    // ---------------------------------------------------------
    private void LogReasons(SmartDecision d)
    {
        foreach (var reason in d.Reasons)
        {
            SmartDecisionLog.Reason(logger, reason);
        }
    }

}


internal static partial class SmartDecisionLog
{
    // ---------------------------------------------------------
    // 2000–2099 — SUBTITLE DECISIONS
    // ---------------------------------------------------------
    [LoggerMessage(EventId = 2000, Level = LogLevel.Information, Message = "Image-based subtitles detected → burn-in required.")]
    public static partial void SubtitlesImageBasedBurnIn(ILogger logger);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Internal subtitles supported → copying.")]
    public static partial void SubtitlesInternalCopy(ILogger logger);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Information, Message = "External subtitles will be extracted.")]
    public static partial void SubtitlesExternalExtract(ILogger logger);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Information, Message = "Subtitle fallback → burn-in required.")]
    public static partial void SubtitlesFallbackBurnIn(ILogger logger);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Information, Message = "Roku: non-SRT subtitle → burn-in required.")]
    public static partial void RokuSubtitleBurnIn(ILogger logger);

    [LoggerMessage(EventId = 2005, Level = LogLevel.Information, Message = "Roku: external SRT supported → extracting.")]
    public static partial void RokuExternalSrt(ILogger logger);

    [LoggerMessage(EventId = 2006, Level = LogLevel.Information, Message = "Roku subtitle check → no override.")]
    public static partial void RokuNoOverride(ILogger logger);

    [LoggerMessage(EventId = 2007, Level = LogLevel.Information, Message = "Anamorphic source detected.")]
    public static partial void AnamorphicDetected(ILogger logger);

    // ---------------------------------------------------------
    // 2100–2199 — AUDIO DECISIONS
    // ---------------------------------------------------------
    [LoggerMessage(EventId = 2100, Level = LogLevel.Information, Message = "DTS/TrueHD detected → transcoding required.")]
    public static partial void AudioDtsTrueHdTranscode(ILogger logger);

    [LoggerMessage(EventId = 2101, Level = LogLevel.Information, Message = "Audio channels exceed client limit.")]
    public static partial void AudioChannelsExceeded(ILogger logger);

    [LoggerMessage(EventId = 2102, Level = LogLevel.Information, Message = "Atmos passthrough allowed.")]
    public static partial void AudioAtmosPassthrough(ILogger logger);

    [LoggerMessage(EventId = 2103, Level = LogLevel.Information, Message = "Atmos unsupported → transcoding required.")]
    public static partial void AudioAtmosTranscode(ILogger logger);

    [LoggerMessage(EventId = 2104, Level = LogLevel.Information, Message = "Multichannel audio → EAC3 selected.")]
    public static partial void AudioMultichannelEac3(ILogger logger);

    [LoggerMessage(EventId = 2105, Level = LogLevel.Information, Message = "AAC stereo fallback required.")]
    public static partial void AudioAacFallback(ILogger logger);

    [LoggerMessage(EventId = 2106, Level = LogLevel.Information, Message = "Stereo audio → AAC selected.")]
    public static partial void AudioStereoAac(ILogger logger);

    // ---------------------------------------------------------
    // 2200–2299 — DIRECT PLAY / DIRECT STREAM
    // ---------------------------------------------------------
    [LoggerMessage(EventId = 2200, Level = LogLevel.Information, Message = "Direct Play allowed.")]
    public static partial void DirectPlayAllowed(ILogger logger);

    [LoggerMessage(EventId = 2201, Level = LogLevel.Information, Message = "Direct Stream allowed.")]
    public static partial void DirectStreamAllowed(ILogger logger);

    [LoggerMessage(EventId = 2202, Level = LogLevel.Information, Message = "Direct Stream audio passthrough.")]
    public static partial void DirectStreamAudioPassthrough(ILogger logger);

    [LoggerMessage(EventId = 2203, Level = LogLevel.Information, Message = "Direct Stream audio transcoding required.")]
    public static partial void DirectStreamAudioTranscode(ILogger logger);

    [LoggerMessage(EventId = 2204, Level = LogLevel.Information, Message = "Direct Stream → downmix to stereo.")]
    public static partial void DirectStreamDownmixStereo(ILogger logger);

    [LoggerMessage(EventId = 2205, Level = LogLevel.Information, Message = "Direct Stream → target audio channels selected.")]
    public static partial void DirectStreamTargetChannels(ILogger logger);

    // ---------------------------------------------------------
    // 2300–2399 — FULL TRANSCODE
    // ---------------------------------------------------------
    [LoggerMessage(EventId = 2300, Level = LogLevel.Information, Message = "Full transcode required.")]
    public static partial void FullTranscodeRequired(ILogger logger);

    [LoggerMessage(EventId = 2301, Level = LogLevel.Information, Message = "Transcode → downmix to stereo.")]
    public static partial void TranscodeDownmixStereo(ILogger logger);

    [LoggerMessage(EventId = 2302, Level = LogLevel.Information, Message = "Transcode → target audio channels selected.")]
    public static partial void TranscodeTargetChannels(ILogger logger);

    [LoggerMessage(EventId = 2303, Level = LogLevel.Information, Message = "Transcode → encoder selected.")]
    public static partial void TranscodeEncoderSelected(ILogger logger);

    [LoggerMessage(EventId = 2304, Level = LogLevel.Information, Message = "Strict H.264 compliance enforced.")]
    public static partial void StrictH264Compliance(ILogger logger);

    // ---------------------------------------------------------
    // 2400–2499 — HDR DECISIONS
    // ---------------------------------------------------------
    [LoggerMessage(EventId = 2400, Level = LogLevel.Information, Message = "HDR disabled for H.264.")]
    public static partial void HdrH264Disabled(ILogger logger);

    [LoggerMessage(EventId = 2401, Level = LogLevel.Information, Message = "HDR disabled for HEVC 8-bit.")]
    public static partial void HdrHevc8BitDisabled(ILogger logger);

    [LoggerMessage(EventId = 2402, Level = LogLevel.Information, Message = "Dolby Vision enabled.")]
    public static partial void HdrDolbyVision(ILogger logger);

    [LoggerMessage(EventId = 2403, Level = LogLevel.Information, Message = "HDR10+ enabled.")]
    public static partial void Hdr10Plus(ILogger logger);

    [LoggerMessage(EventId = 2404, Level = LogLevel.Information, Message = "HDR10 enabled.")]
    public static partial void Hdr10(ILogger logger);

    [LoggerMessage(EventId = 2405, Level = LogLevel.Information, Message = "HDR → SDR fallback.")]
    public static partial void HdrSdrFallback(ILogger logger);

    // ---------------------------------------------------------
    // 2500–2599 — SCALING DECISIONS
    // ---------------------------------------------------------
    [LoggerMessage(EventId = 2500, Level = LogLevel.Information, Message = "Scaling required.")]
    public static partial void ScalingRequired(ILogger logger);

    [LoggerMessage(EventId = 2501, Level = LogLevel.Information, Message = "Strict resolution enforced.")]
    public static partial void StrictResolutionEnforced(ILogger logger);

    // ---------------------------------------------------------
    // 2600–2699 — HARDWARE ENCODER SELECTION
    // ---------------------------------------------------------
    // (Handled by TranscodeEncoderSelected)

    // ---------------------------------------------------------
    // 2700–2799 — FINAL SUMMARY (one log per reason)
    // ---------------------------------------------------------
    [LoggerMessage(EventId = 2700, Level = LogLevel.Information, Message = "Decision reason: {Reason}")]
    public static partial void Reason(ILogger logger, string Reason);
}
