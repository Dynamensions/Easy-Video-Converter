using System.Text.Json.Serialization;

namespace Dynamensions.EasyVideoConverter.Core.Models.Ffprobe;

public class FfprobeTags
{
    [JsonPropertyName("language")]
    public string Language { get; set; }

    [JsonPropertyName("BPS")]
    public string BPS { get; set; }

    [JsonPropertyName("DURATION")]
    public string DURATION { get; set; }

    [JsonPropertyName("NUMBER_OF_FRAMES")]
    public string NUMBEROFFRAMES { get; set; }

    [JsonPropertyName("NUMBER_OF_BYTES")]
    public string NUMBEROFBYTES { get; set; }

    [JsonPropertyName("_STATISTICS_WRITING_APP")]
    public string STATISTICSWRITINGAPP { get; set; }

    [JsonPropertyName("_STATISTICS_WRITING_DATE_UTC")]
    public string STATISTICSWRITINGDATEUTC { get; set; }

    [JsonPropertyName("_STATISTICS_TAGS")]
    public string STATISTICSTAGS { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("encoder")]
    public string Encoder { get; set; }

    [JsonPropertyName("creation_time")]
    public DateTime CreationTime { get; set; }

    [JsonPropertyName("IMDB")]
    public string IMDB { get; set; }

    [JsonPropertyName("TMDB")]
    public string TMDB { get; set; }

    [JsonPropertyName("TVDB")]
    public string TVDB { get; set; }
}