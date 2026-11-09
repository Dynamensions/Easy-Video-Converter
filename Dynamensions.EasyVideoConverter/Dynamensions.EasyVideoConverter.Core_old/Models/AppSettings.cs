namespace Dynamensions.EasyVideoConverter.Core.Models;

public class AppSettings
{
    public string? FFprobePath { get; set; } = "ffprobe.exe";
    public string FFmpegPath { get; set; } = "ffmpeg.exe";


    // Video
    public bool UpscaleVideo { get; set; }

    public string OutputFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

    public string? SelectedEncoderProfileName { get; set; }
    public string? SelectedClientProfileName { get; set; }
    public List<string> PreferredSubtitleLanguages { get; set; } = [];
    public string? PreferredBurninLanguage { get; set; } = null;
    public string FilenamePattern { get; set; } = "{name}";
    public string SelectedPatternPreset { get; set; } = "Simple";

    public int DefaultCrf { get; set; } = 18;

}