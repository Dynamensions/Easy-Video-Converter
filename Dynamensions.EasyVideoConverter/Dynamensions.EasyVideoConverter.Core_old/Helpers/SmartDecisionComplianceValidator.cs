using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Models.Enums;
using Dynamensions.EasyVideoConverter.Core.Models.Profiles;
using Dynamensions.EasyVideoConverter.Core.Services;

namespace Dynamensions.EasyVideoConverter.Core.Helpers;

public class SmartDecisionComplianceValidator(IMediaFileAnalyzerService ffprobe)
{

    /// <summary>
    /// Validates that the encoded file matches the SmartDecision output
    /// and represents the lowest-impact format for the server.
    /// </summary>
    public async Task<SmartDecisionComplianceResult> ValidateAsync(
        string outputPath,
        ClientProfile client,
        SmartDecision decision,
        CancellationToken cancellationToken)
    {
        var info = await ffprobe.AnalyzeAsync(outputPath, cancellationToken);

        var fi = new FileInfo(outputPath);
        var file = new MediaFile() { Path = outputPath, SizeBytes = fi.Length };

        var result = new SmartDecisionComplianceResult
        {
            OutputPath = outputPath,
            ClientName = client.Name,
            MediaInfo = info,
            MediaFile = file
        };

        // ---------------------------------------------------------
        // 1. Container
        // ---------------------------------------------------------
        var normalizedContainer = ContainerNormalizer.Normalize(info.Container);
        if (ContainerNormalizer.Normalize(info.Container) != decision.TargetContainer)
            result.Reasons.Add($"Container mismatch: expected {decision.TargetContainer}, got {normalizedContainer}");

        // ---------------------------------------------------------
        // 2. Video codec
        // ---------------------------------------------------------
        var videoCodec = VideoCodecNormalizer.Normalize(decision.SelectedVideoCodec);
        if (videoCodec != decision.SelectedVideoCodec)
            result.Reasons.Add($"Video codec mismatch: expected {decision.SelectedVideoCodec}, got {videoCodec}");

        // ---------------------------------------------------------
        // 3. Audio codec
        // ---------------------------------------------------------
        var audioCodec = AudioCodecNormalizer.Normalize(decision.SelectedAudioCodec);
        if (audioCodec != decision.SelectedAudioCodec)
            result.Reasons.Add($"Audio codec mismatch: expected {decision.SelectedAudioCodec}, got {audioCodec}");

        // ---------------------------------------------------------
        // 4. Resolution (if scaling was required)
        // ---------------------------------------------------------
        if (decision.RequiresScaling)
        {
            if (info.Width != decision.TargetWidth || info.Height != decision.TargetHeight)
            {
                result.Reasons.Add(
                    $"Resolution mismatch: expected {decision.TargetWidth}x{decision.TargetHeight}, got {info.Width}x{info.Height}");
            }
        }

        // ---------------------------------------------------------
        // 5. SAR fix (square pixels)
        // ---------------------------------------------------------
        if (decision.RequiresSarFix && info.PixelAspectRatio != 1.0)
            result.Reasons.Add($"SAR mismatch: expected square pixels (PAR=1.0), got PAR={info.PixelAspectRatio}");

        // ---------------------------------------------------------
        // 6. Strict H.264 compliance (if required)
        // ---------------------------------------------------------
        if (client.RequiresStrictH264Compliance && info.VideoCodec == "h264")
        {
            if (!string.Equals(info.H264Profile, client.RequiredProfile, StringComparison.OrdinalIgnoreCase))
                result.Reasons.Add($"Profile mismatch: expected {client.RequiredProfile}, got {info.H264Profile}");

            if (info.H264Level == null || info.H264Level.Value.ToString("0.0") != client.RequiredLevel)
                result.Reasons.Add($"Level mismatch: expected {client.RequiredLevel}, got {info.H264Level:0.0}");
        }

        // ---------------------------------------------------------
        // 7. Bit depth (8-bit clients)
        // ---------------------------------------------------------
        if (client.Requires8Bit && info.BitDepth > 8)
            result.Reasons.Add($"Bit depth mismatch: expected 8-bit, got {info.BitDepth}");

        // ---------------------------------------------------------
        // 8. Subtitle handling
        // ---------------------------------------------------------
        if (decision.SubtitleHandling == SubtitleMode.BurnIn)
        {
            if (info.HasSubtitles)
                result.Reasons.Add("Burn-in mismatch: expected burned-in subtitles, but soft subtitles were detected.");
        }

        // ---------------------------------------------------------
        // Final result
        // ---------------------------------------------------------
        result.IsCompliant = result.Reasons.Count == 0;
        return result;
    }
}

public class SmartDecisionComplianceResult
{
    public string OutputPath { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public MediaInfo MediaInfo { get; set; } = new();
    public MediaFile MediaFile { get; set; } = new();
    public bool IsCompliant { get; set; }
    public List<string> Reasons { get; } = [];

    public override string ToString()
    {
        if (IsCompliant)
            return $"Output validated for {ClientName}: SmartDecision compliance OK.";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"SmartDecision compliance FAILED for {ClientName}:");
        foreach (var r in Reasons)
            sb.AppendLine($" - {r}");
        return sb.ToString();
    }
}
