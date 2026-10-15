namespace Dynamensions.EasyVideoConverter.Core.Models.Enums;

public enum AudioTrackSelectionMode
{
    /// <summary>
    /// Always select the first audio track.
    /// </summary>
    First,

    /// <summary>
    /// Select English audio if available, otherwise fall back to first track.
    /// </summary>
    EnglishPreferred,

    /// <summary>
    /// Select the highest bitrate audio track.
    /// </summary>
    BestQuality,

    /// <summary>
    /// Prefer 5.1/7.1 surround audio, otherwise fall back to first track.
    /// </summary>
    SurroundPreferred,

    /// <summary>
    /// Include all audio tracks in the output.
    /// </summary>
    All
}
