namespace Dynamensions.EasyVideoConverter.Core.Models.Ffprobe;

using System.Text.Json.Serialization;

public class FfprobeRoot
{
    [JsonPropertyName("streams")]
    public List<FfprobeStream> Streams { get; set; } = [];

    [JsonPropertyName("format")]
    public FfprobeFormat Format { get; set; } = new();
}
