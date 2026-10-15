namespace Dynamensions.EasyVideoConverter.Core.Helpers;

public static class ContainerNormalizer
{
    public static string Normalize(string? container)
    {
        if (string.IsNullOrWhiteSpace(container))
            return "unknown";

        container = container.ToLowerInvariant();

        // MP4 family
        if (container.Contains("mp4") ||
            container.Contains("mov") ||
            container.Contains("m4a") ||
            container.Contains("3gp") ||
            container.Contains("3g2"))
            return "mp4";

        // MKV family
        if (container.Contains("mkv") ||
            container.Contains("matroska") ||
            container.Contains("webm"))
            return "mkv";

        // AVI
        if (container.Contains("avi"))
            return "avi";

        // MPEG-TS
        if (container.Contains("mpegts") ||
            container.Contains("ts"))
            return "ts";

        return container;
    }
}

public static class VideoCodecNormalizer
{
    public static string Normalize(string? codec)
    {
        if (string.IsNullOrWhiteSpace(codec))
            return "unknown";

        codec = codec.ToLowerInvariant();

        if (codec.Contains("h264") || codec.Contains("avc"))
            return "h264";

        if (codec.Contains("hevc") || codec.Contains("h265"))
            return "hevc";

        if (codec.Contains("av1"))
            return "av1";

        if (codec.Contains("vp9"))
            return "vp9";

        if (codec.Contains("mpeg4"))
            return "mpeg4";

        return codec;
    }
}

public static class AudioCodecNormalizer
{
    public static string Normalize(string? codec)
    {
        if (string.IsNullOrWhiteSpace(codec))
            return "unknown";

        codec = codec.ToLowerInvariant();

        if (codec.Contains("aac"))
            return "aac";

        if (codec.Contains("ac3"))
            return "ac3";

        if (codec.Contains("eac3"))
            return "eac3";

        if (codec.Contains("opus"))
            return "opus";

        if (codec.Contains("flac"))
            return "flac";

        if (codec.Contains("mp3"))
            return "mp3";

        return codec;
    }
}

public static class PixelFormatNormalizer
{
    public static string Normalize(string? pixFmt)
    {
        if (string.IsNullOrWhiteSpace(pixFmt))
            return "unknown";

        pixFmt = pixFmt.ToLowerInvariant();

        // Common formats
        if (pixFmt.Contains("yuv420p"))
            return "yuv420p";

        if (pixFmt.Contains("yuv420p10"))
            return "yuv420p10";

        if (pixFmt.Contains("p010"))
            return "p010";

        if (pixFmt.Contains("nv12"))
            return "nv12";

        return pixFmt;
    }
}

public static class AudioLayoutNormalizer
{
    public static string Normalize(string? layout)
    {
        if (string.IsNullOrWhiteSpace(layout))
            return "unknown";

        layout = layout.ToLowerInvariant();

        if (layout.Contains("stereo") || layout == "2.0")
            return "stereo";

        if (layout.Contains("5.1"))
            return "5.1";

        if (layout.Contains("7.1"))
            return "7.1";

        return layout;
    }
}

public static class SubtitleCodecNormalizer
{
    public static string Normalize(string? codec)
    {
        if (string.IsNullOrWhiteSpace(codec))
            return "unknown";

        codec = codec.ToLowerInvariant();

        if (codec.Contains("subrip") || codec.Contains("srt"))
            return "srt";

        if (codec.Contains("ass"))
            return "ass";

        if (codec.Contains("pgs"))
            return "pgs";

        if (codec.Contains("mov_text"))
            return "mov_text";

        if (codec.Contains("dvd_subtitle") || codec.Contains("vobsub"))
            return "vobsub";

        return codec;
    }
}
