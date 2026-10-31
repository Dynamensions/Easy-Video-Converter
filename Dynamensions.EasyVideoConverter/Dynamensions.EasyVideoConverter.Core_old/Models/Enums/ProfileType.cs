namespace Dynamensions.EasyVideoConverter.Core.Models.Enums;

public enum ProfileType
{
    /// <summary>
    /// Streaming devices like Roku, Fire TV, Apple TV, Smart TVs, and Plex clients.
    /// </summary>
    Streaming,

    /// <summary>
    /// Archival storage for high-quality MKV files with lossless audio and extracted subtitles.
    /// </summary>
    Archival,

    /// <summary>
    /// Peer-to-peer or small file distribution with HEVC/AV1 encoding and smaller output size.
    /// </summary>
    P2P,

    /// <summary>
    /// Optimization for mobile devices with lower resolution and bitrate for fast decoding.
    /// </summary>
    Mobile,

    /// <summary>
    /// Optimization for low bandwidth scenarios with aggressive bitrate caps and lower resolutions.
    /// </summary>
    LowBandwidth,

    /// <summary>
    /// High quality profiles with slow presets, high bitrate, and visually optimized settings.
    /// </summary>
    HighQuality
}
