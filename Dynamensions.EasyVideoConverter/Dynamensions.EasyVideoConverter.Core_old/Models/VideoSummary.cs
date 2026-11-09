namespace Dynamensions.EasyVideoConverter.Core.Models;

public class VideoSummary
{
    public required string Codec { get; set; } = "";
    public required string Profile { get; set; } = "";
    public required string Level { get; set; } = "";
    public required string BitDepth { get; set; } = "";
    public required string Color { get; set; } = "";   // SDR, HDR10, HDR10+, DV

    public required int Width { get; set; }
    public required int Height { get; set; }

    public required bool IsHdr10 { get; set; }
    public required bool IsHdr10Plus { get; set; }
    public required bool IsDolbyVision { get; set; }

    public override string ToString()
    {
        List<string> items =
            [
            Codec,
            Profile,
            Level,
            BitDepth,
            Color
            ];


        return String.Join(',', items.Where(x => !String.IsNullOrWhiteSpace(x)));
    }
}
