namespace Dynamensions.EasyVideoConverter.Core.Models;

public class AudioSummary
{
    public required string Codec { get; set; } = string.Empty;
    public required int Channels { get; set; }
    public required int AudioBitrateKbps { get; set; }
    public required bool IsAtmos { get; set; }
    public required bool RequiresAacStereoFallback { get; set; }

    public override string ToString()
    {

        var layout = GetChannelDefinition();
        var desc = $"{Codec}, {layout}, {AudioBitrateKbps} Kpbs";

        if (IsAtmos) desc += ", Atmos";

        if (RequiresAacStereoFallback) desc += ", aac 2 fallback";

        return desc;
    }

    public string GetChannelDefinition()
    {
        string layout = Channels switch
        {
            1 => "1 (Mono)",
            2 => "2 (Stereo)",
            6 => "5.1 (Surround)",
            8 => "7.1 (Surround)",
            _ => $"{Channels} ch"
        };

        return layout;
    }

    public double GetChannelTag()
    {
        double layout = Channels switch
        {
            1 => 1,
            2 => 2,
            6 => 5.1,
            8 => 7.1,
            10 => 9.1,
            _ => Channels
        };

        return layout;
    }
}
