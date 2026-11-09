namespace Dynamensions.EasyVideoConverter.Core.Models.Enums;

public enum SubtitleMode
{
    /// <summary>
    /// Copy text-based subtitles (SRT/ASS) directly into the output container.
    /// </summary>
    Copy,

    ///// <summary>
    ///// Convert text-based subtitles (SRT/ASS) into the output container format.
    ///// </summary>
    //Convert,

    /// <summary>
    /// Render subtitles into the video stream (required for PGS or forced subs).
    /// </summary>
    BurnIn,

    /// <summary>
    /// Extract subtitles into external .srt files.
    /// </summary>
    Extract,

    /// <summary>
    /// Do not include subtitles in the output.
    /// </summary>
    None
}
