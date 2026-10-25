using Dynamensions.EasyVideoConverter.Core.Models;
using Microsoft.Extensions.Logging;
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

public class TranscodeQueueService : ITranscodeQueueService
{
    private readonly ObservableCollection<QueueItem> _items = [];
    private readonly ILogger<TranscodeQueueService> logger;

    public ReadOnlyObservableCollection<QueueItem> Items { get; }

    public TranscodeQueueService(ILogger<TranscodeQueueService> logger)
    {
        Items = new(_items);
        this.logger = logger;
    }

    public void Add(TranscodeTask transcodeTask, double estimatedConversionSeconds)
    {
        TranscodeQueueLog.QueueAddBegin(logger, transcodeTask.File.Path);

        _items.Add(new QueueItem
        {
            File = transcodeTask.File,
            Info = transcodeTask.Info,
            ServerProfile = transcodeTask.Server,
            //Decision = transcodeTask.Decision,
            Status = "Pending",
            Progress = 0,
            EstimatedTimeRemaining = TimeSpan.FromSeconds(estimatedConversionSeconds),
            Task = transcodeTask,
        });

        TranscodeQueueLog.QueueAddComplete(logger, transcodeTask.File.Path);
    }


    public void Remove(QueueItem item)
    {
        TranscodeQueueLog.QueueRemove(logger, item.File.Path);
        _items.Remove(item);
    }

    public void ClearAll()
    {
        TranscodeQueueLog.QueueClearAll(logger, _items.Count);
        _items.Clear();
    }

    public void RemoveCompleted()
    {
        var completedItems = _items.Where(i => i.IsCompleted).ToList();

        TranscodeQueueLog.QueueRemoveCompletedBegin(logger, completedItems.Count);

        foreach (var item in completedItems)
        {
            _items.Remove(item);
        }

        TranscodeQueueLog.QueueRemoveCompletedEnd(logger);
    }

    public void RemoveFailed()
    {
        var failedItems = _items.Where(i => i.Status == "Failed").ToList();

        TranscodeQueueLog.QueueRemoveFailedBegin(logger, failedItems.Count);

        foreach (var item in failedItems)
        {
            _items.Remove(item);
        }

        TranscodeQueueLog.QueueRemoveFailedEnd(logger);
    }

}

internal static partial class TranscodeQueueLog
{
    // ---------------------------------------------------------
    // 3000–3099 — QUEUE MUTATIONS
    // ---------------------------------------------------------

    [LoggerMessage(EventId = 3000, Level = LogLevel.Information, Message = "Adding item to queue: {Path}")]
    public static partial void QueueAddBegin(ILogger logger, string Path);

    [LoggerMessage(EventId = 3001, Level = LogLevel.Information, Message = "Item added to queue: {Path}")]
    public static partial void QueueAddComplete(ILogger logger, string Path);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Information, Message = "Removing item from queue: {Path}")]
    public static partial void QueueRemove(ILogger logger, string Path);

    [LoggerMessage(EventId = 3003, Level = LogLevel.Information, Message = "Clearing all queue items (count={Count}).")]
    public static partial void QueueClearAll(ILogger logger, int Count);

    // ---------------------------------------------------------
    // 3100–3199 — STATUS-BASED REMOVALS
    // ---------------------------------------------------------

    [LoggerMessage(EventId = 3100, Level = LogLevel.Information, Message = "Removing completed items (count={Count}).")]
    public static partial void QueueRemoveCompletedBegin(ILogger logger, int Count);

    [LoggerMessage(EventId = 3101, Level = LogLevel.Information, Message = "Removing completed item: {Path}")]
    public static partial void QueueRemoveCompletedItem(ILogger logger, string Path);

    [LoggerMessage(EventId = 3102, Level = LogLevel.Information, Message = "Completed item removal finished.")]
    public static partial void QueueRemoveCompletedEnd(ILogger logger);

    [LoggerMessage(EventId = 3103, Level = LogLevel.Information, Message = "Removing failed items (count={Count}).")]
    public static partial void QueueRemoveFailedBegin(ILogger logger, int Count);

    [LoggerMessage(EventId = 3104, Level = LogLevel.Information, Message = "Removing failed item: {Path}")]
    public static partial void QueueRemoveFailedItem(ILogger logger, string Path);

    [LoggerMessage(EventId = 3105, Level = LogLevel.Information, Message = "Failed item removal finished.")]
    public static partial void QueueRemoveFailedEnd(ILogger logger);
}
