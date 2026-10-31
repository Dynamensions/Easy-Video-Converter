using Dynamensions.EasyVideoConverter.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamensions.EasyVideoConverter.Core.Services;

public interface ISubtitleExtractorService
{
    Task<string?> ExtractForBurnInAsync(QueueItem item, Action<(QueueItem item, double ProgressValue, double CurrentFPS, double speedX, double secondsRemaining)>? progressCallback, CancellationToken cancellationToken);
    Task ExportAllAsync(QueueItem item,
        IEnumerable<SubtitleTrack> tracks,
        Action<(QueueItem item, double ProgressValue, double CurrentFPS, double speedX, double secondsRemaining)>? progressCallback,
        Action<(QueueItem item, int CurrentTrack, int TotalTracks, SubtitleTrack subtitleTrack)>? trackCallback,
        CancellationToken cancellationToken);

    Task ExportAsync(QueueItem item, SubtitleTrack track, Action<(QueueItem item, double ProgressValue, double CurrentFPS, double speedX, double secondsRemaining)>? progressCallback, CancellationToken cancellationToken);
}
