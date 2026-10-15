using Dynamensions.EasyVideoConverter.Core.Models;

namespace Dynamensions.EasyVideoConverter.Core.Services;

public interface ISubtitleExtractorService
{
    Task<string?> ExtractForBurnInAsync(QueueItem item, Action<(QueueItem item, double ProgressValue, double CurrentFPS, double speedX, double secondsRemaining)>? progressCallback);
    Task ExportAllAsync(QueueItem item,
        IEnumerable<SubtitleTrack> tracks,
        Action<(QueueItem item, double ProgressValue, double CurrentFPS, double speedX, double secondsRemaining)>? progressCallback,
        Action<(QueueItem item, int CurrentTrack, int TotalTracks, SubtitleTrack subtitleTrack)>? trackCallback);

    Task ExportAsync(QueueItem item, SubtitleTrack track, Action<(QueueItem item, double ProgressValue, double CurrentFPS, double speedX, double secondsRemaining)>? progressCallback);
}
