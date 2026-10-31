using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Models.Enums;
using Dynamensions.EasyVideoConverter.Core.Models.Profiles;
using System.Text.RegularExpressions;

namespace Dynamensions.EasyVideoConverter.Core.Helpers;

public static partial class FilenamePatternEngine
{
    private static readonly Regex InvalidChars =
        new($"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", RegexOptions.Compiled);

    public static string Apply(
        string? pattern,
        string originalFilePath,
        VideoSummary videoSummary,
        AudioSummary audioSummary,
        ClientProfile client,
        EncoderProfile encoder,
        SubtitleMode subtitleMode,
        SubtitleTrack? burnInSubtitle,
        IEnumerable<SubtitleTrack> externalSubtitles,
        MediaInfo mediaInfo)
    {
        if (string.IsNullOrWhiteSpace(pattern)) return string.Empty;
        var name = Path.GetFileNameWithoutExtension(originalFilePath);
        var ext = Path.GetExtension(originalFilePath).TrimStart('.');

        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = name,
            ["ext"] = ext,

            // Video
            ["vcodec"] = videoSummary?.Codec ?? "",
            ["profile"] = videoSummary?.Profile ?? "",
            ["level"] = videoSummary?.Level ?? "",
            ["bitdepth"] = videoSummary?.BitDepth ?? "",
            ["hdr"] = GetHdrTag(videoSummary),
            ["res"] = $"{videoSummary?.Width}x{videoSummary?.Height}",
            ["fps"] = (Math.Truncate(100 * mediaInfo?.FrameRate ?? 0) / 100).ToString("0.##") ?? "",

            // Audio
            ["acodec"] = audioSummary?.Codec ?? "",
            ["abitrate"] = audioSummary?.AudioBitrateKbps.ToString() ?? "",
            ["achannels"] = audioSummary?.GetChannelTag().ToString() ?? "",

            // Subtitles
            ["sub"] = GetSubtitleTag(subtitleMode, burnInSubtitle, externalSubtitles),

            // Client / Encoder
            ["client"] = client?.Name ?? "",
            ["encoder"] = encoder?.Name ?? "",

            // SmartDecision / Performance
            ["perf"] = GetPerformanceKey(videoSummary, subtitleMode),

            // Misc
            ["date"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["folder"] = GetFolderTag(originalFilePath)
        };

        string output = ReplaceTokens(pattern, tokens);
        output = Sanitize(output);

        return output;
    }

    private static string ReplaceTokens(string pattern, Dictionary<string, string> tokens)
    {
        return FilenameRegex().Replace(pattern, match =>
        {
            var key = match.Groups[1].Value;
            return tokens.TryGetValue(key, out var value) ? value : "";
        });
    }

    private static string Sanitize(string filename)
    {
        filename = InvalidChars.Replace(filename, "_");
        filename = filename.Replace("..", ".");
        filename = filename.Trim('.', '_', ' ');
        return filename;
    }

    private static string GetHdrTag(VideoSummary? summary)
    {
        if (summary == null) return "";

        if (summary.IsDolbyVision) return "DV";
        if (summary.IsHdr10Plus) return "HDR10+";
        if (summary.IsHdr10) return "HDR10";
        if (summary.BitDepth == "10bit") return "HDR10bit";

        return "SDR";
    }

    private static string GetSubtitleTag(SubtitleMode mode, SubtitleTrack? burnInSubtitle, IEnumerable<SubtitleTrack>? externalTracks)
    {
        return mode switch
        {
            SubtitleMode.BurnIn => $"burnin-{burnInSubtitle?.Language ?? "unk"}",
            SubtitleMode.Extract => $"ext-{externalTracks?.Count() ?? 0}subs",
            _ => "nosubs"
        };
    }

    private static string GetPerformanceKey(VideoSummary? summary, SubtitleMode mode)
    {
        if (summary == null) return "";

        return $"{summary.Codec}-{summary.Profile}-{summary.Width}x{summary.Height}-{summary.BitDepth}-{mode}";
    }

    private static string GetFolderTag(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (string.IsNullOrWhiteSpace(dir)) return "";
        var root = Path.GetPathRoot(dir) ?? "";
        return dir.Replace(root, "").Trim(Path.DirectorySeparatorChar);
    }

    [GeneratedRegex(@"\{([a-zA-Z0-9_]+)\}")]
    private static partial Regex FilenameRegex();
}