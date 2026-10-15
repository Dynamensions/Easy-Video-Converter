using Dynamensions.EasyVideoConverter.Core.Helpers;

namespace Dynamensions.EasyVideoConverter.Core.Services;

public interface ISystemService
{

    EncoderCapabilities GetEncoderCapabilities();
    bool HasFfmpegEncoder(string ffmpegPath, string encoderName);
}
