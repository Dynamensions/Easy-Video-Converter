namespace Dynamensions.EasyVideoConverter.Core.Services;

public sealed class EncoderCapabilities
{
    public bool HasCpu { get; init; } = true;
    public bool HasNvidia { get; init; }
    public bool HasIntelQsv { get; init; }
    public bool HasAmdAmf { get; init; }
}

public interface ISystemService
{
    EncoderCapabilities GetEncoderCapabilities();
    bool HasFfmpegEncoder(string ffmpegPath, string encoderName);
    Task<bool> CopyFileAsync(string sourcePath,
        string destinationPath,
        bool overwriteExistingFile = false,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);
}
