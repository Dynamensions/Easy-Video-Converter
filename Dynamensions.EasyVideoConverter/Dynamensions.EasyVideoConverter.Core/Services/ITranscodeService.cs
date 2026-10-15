using Dynamensions.EasyVideoConverter.Core.Models;

namespace Dynamensions.EasyVideoConverter.Core.Services;

public interface ITranscodeService
{
    Task TranscodeAsync(QueueItem queueItem, Action<(QueueItem item, double ProgressValue, double CurrentFPS, double speedX, double secondsRemaining)>? progressCallback);
}
