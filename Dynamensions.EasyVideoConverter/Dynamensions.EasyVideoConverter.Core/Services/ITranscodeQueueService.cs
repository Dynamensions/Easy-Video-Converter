using Dynamensions.EasyVideoConverter.Core.Models;
using System.Collections.ObjectModel;

namespace Dynamensions.EasyVideoConverter.Core.Services;

public interface ITranscodeQueueService
{
    ReadOnlyObservableCollection<QueueItem> Items { get; }

    void Add(TranscodeTask transcodeTask, double estimatedConversionSeconds);
    void Remove(QueueItem item);
    void ClearAll();
    void RemoveCompleted();
    void RemoveFailed();
}
