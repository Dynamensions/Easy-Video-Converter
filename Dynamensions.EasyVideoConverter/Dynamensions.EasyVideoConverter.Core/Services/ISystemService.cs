using Dynamensions.EasyVideoConverter.Core.Helpers;

namespace Dynamensions.EasyVideoConverter.Core.Services;

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
