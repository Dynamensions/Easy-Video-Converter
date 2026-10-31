using System;

namespace Dynamensions.EasyVideoConverter.Core.Models;

public class SubtitleTrack
{
    public int Index { get; set; }
    public int FfmpegIndex { get; set; }

    public string Codec { get; set; } = "unknown";
    public string Language { get; set; } = "und";
    public string? Title { get; set; }

    public bool IsForced =>
        Title?.Contains("forced", StringComparison.OrdinalIgnoreCase) == true;

    public bool IsPgs => Codec == "hdmv_pgs_subtitle";
    public bool IsSrt => Codec == "subrip";
    public bool IsAss => Codec == "ass";

    public override string ToString()
    {
        return
           $"Index: {Index}\n" +
           $"FfmpegIndex: {FfmpegIndex}\n" +
           $"Coded: {Codec}\n" +
           $"Language: {Language}\n" +
           $"Title: {Title}\n" +
           $"IsForced: {IsForced}\n" +
           $"IsPgs: {IsPgs}\n" +
           $"IsSrt: {IsSrt}\n" +
           $"IsAss: {IsAss}\n";
    }
}
