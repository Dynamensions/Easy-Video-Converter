using System.Text.Json.Serialization;

namespace Dynamensions.EasyVideoConverter.Core.Models.Ffprobe;

public class FfprobeStream
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("codec_name")]
    public string CodecName { get; set; }

    [JsonPropertyName("codec_long_name")]
    public string CodecLongName { get; set; }

    [JsonPropertyName("profile")]
    public string Profile { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("codec_type")]
    public string CodecType { get; set; }

    [JsonPropertyName("codec_tag_string")]
    public string CodecTagString { get; set; }

    [JsonPropertyName("codec_tag")]
    public string CodecTag { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("coded_width")]
    public int CodedWidth { get; set; }

    [JsonPropertyName("coded_height")]
    public int CodedHeight { get; set; }

    [JsonPropertyName("has_b_frames")]
    public int HasBFrames { get; set; }

    [JsonPropertyName("sample_aspect_ratio")]
    public string SampleAspectRatio { get; set; }

    [JsonPropertyName("display_aspect_ratio")]
    public string DisplayAspectRatio { get; set; }

    [JsonPropertyName("pix_fmt")]
    public string PixFmt { get; set; }

    [JsonPropertyName("color_range")]
    public string ColorRange { get; set; }

    [JsonPropertyName("color_space")]
    public string ColorSpace { get; set; }

    [JsonPropertyName("color_transfer")]
    public string ColorTransfer { get; set; }

    [JsonPropertyName("color_primaries")]
    public string ColorPrimaries { get; set; }

    [JsonPropertyName("chroma_location")]
    public string ChromaLocation { get; set; }

    [JsonPropertyName("view_ids_available")]
    public string ViewIdsAvailable { get; set; }

    [JsonPropertyName("view_pos_available")]
    public string ViewPosAvailable { get; set; }

    [JsonPropertyName("r_frame_rate")]
    public string RFrameRate { get; set; }

    [JsonPropertyName("avg_frame_rate")]
    public string AvgFrameRate { get; set; }

    [JsonPropertyName("time_base")]
    public string TimeBase { get; set; }

    [JsonPropertyName("start_pts")]
    public int StartPts { get; set; }

    [JsonPropertyName("start_time")]
    public string StartTime { get; set; }

    [JsonPropertyName("extradata_size")]
    public int ExtradataSize { get; set; }

    [JsonPropertyName("disposition")]
    public FfprobeDisposition Disposition { get; set; }

    [JsonPropertyName("tags")]
    public FfprobeTags Tags { get; set; }

    [JsonPropertyName("side_data_list")]
    public List<FfprobeSideData> SideDataList { get; set; }

    [JsonPropertyName("sample_fmt")]
    public string SampleFmt { get; set; }

    [JsonPropertyName("sample_rate")]
    public string SampleRate { get; set; }

    [JsonPropertyName("channels")]
    public int? Channels { get; set; }

    [JsonPropertyName("channel_layout")]
    public string ChannelLayout { get; set; }

    [JsonPropertyName("bits_per_sample")]
    public int? BitsPerSample { get; set; }

    [JsonPropertyName("initial_padding")]
    public int? InitialPadding { get; set; }

    [JsonPropertyName("bits_per_raw_sample")]
    public string BitsPerRawSample { get; set; }

    [JsonPropertyName("mime_codec_string")]
    public string MimeCodecString { get; set; }

    [JsonPropertyName("dmix_mode")]
    public string DmixMode { get; set; }

    [JsonPropertyName("ltrt_cmixlev")]
    public string LtrtCmixlev { get; set; }

    [JsonPropertyName("ltrt_surmixlev")]
    public string LtrtSurmixlev { get; set; }

    [JsonPropertyName("loro_cmixlev")]
    public string LoroCmixlev { get; set; }

    [JsonPropertyName("loro_surmixlev")]
    public string LoroSurmixlev { get; set; }

    [JsonPropertyName("bit_rate")]
    public string BitRate { get; set; }

    [JsonPropertyName("duration_ts")]
    public int? DurationTs { get; set; }

    [JsonPropertyName("duration")]
    public string Duration { get; set; }
}