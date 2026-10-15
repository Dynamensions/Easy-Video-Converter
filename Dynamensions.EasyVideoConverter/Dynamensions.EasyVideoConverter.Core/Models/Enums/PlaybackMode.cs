namespace Dynamensions.EasyVideoConverter.Core.Models.Enums;

public enum PlaybackMode
{
    /// <summary>
    /// No transcoding required; container, codecs, and streams are compatible.
    /// </summary>
    DirectPlay,

    /// <summary>
    /// Only remuxing or minor changes (e.g., audio re-encode) are required.
    /// </summary>
    DirectStream,

    /// <summary>
    /// Video and/or audio must be re-encoded.
    /// </summary>
    Transcode
}
