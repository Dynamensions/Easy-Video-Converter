using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Dynamensions.EasyVideoConverter.Services;

public sealed partial class PerformanceBucketService(ILogger<PerformanceBucketService> logger) : IPerformanceBucketService
{
    // Thread-safe bucket storage
    private readonly ConcurrentDictionary<string, PerformanceBucket> _buckets = new();


    // ---------------------------------------------------------
    // GET BUCKET
    // ---------------------------------------------------------
    public PerformanceBucket? GetBucket(PerformanceBucketKey key)
    {
        if (key == null)
        {
            LogNullKey();
            return null;
        }

        var bucketKey = key.ToKeyString();

        if (_buckets.TryGetValue(bucketKey, out var bucket))
        {
            LogBucketFound(bucketKey, bucket.SampleCount);
            return bucket;
        }

        LogBucketMissing(bucketKey);
        return null;
    }

    // ---------------------------------------------------------
    // RECORD SAMPLE
    // ---------------------------------------------------------
    public void RecordJob(PerformanceBucketKey key, double inputDurationSeconds, double encodeDurationSeconds)
    {
        if (key == null)
        {
            LogNullKey();
            return;
        }

        var bucketKey = key.ToKeyString();

        var bucket = _buckets.GetOrAdd(bucketKey, _ =>
        {
            LogBucketCreated(bucketKey);
            return new PerformanceBucket();
        });

        bucket.AddSample(inputDurationSeconds, encodeDurationSeconds);

        LogSampleRecorded(bucketKey, inputDurationSeconds, encodeDurationSeconds, bucket.SampleCount);
    }

    // ---------------------------------------------------------
    // ESTIMATE ENCODE SPEED (seconds per second)
    // ---------------------------------------------------------
    public double EstimateSecondsPerSecond(PerformanceBucketKey key)
    {
        var bucket = GetBucket(key);

        if (bucket == null || bucket.SampleCount == 0)
        {
            LogSpeedFallback(key?.ToKeyString() ?? "null");
            return 0.95;
        }

        var sps = bucket.SecondsPerSecond;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            var arg = key.ToKeyString() ?? "null";
            LogSpeedEstimated(arg, sps);
        }

        return sps;
    }

    // ---------------------------------------------------------
    // ESTIMATE SERVER LOAD (0–100%)
    // ---------------------------------------------------------
    public int EstimateLoadPercent(PerformanceBucketKey key)
    {
        var bucket = GetBucket(key);

        if (bucket == null)
        {
            LogLoadFallback(key?.ToKeyString() ?? "null");
            return 95;
        }

        var load = bucket.EstimatedLoadPercent;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            var arg = key.ToKeyString() ?? "null";
            LogLoadEstimated(arg, load);
        }
        return load;
    }

    // ---------------------------------------------------------
    // SOURCE-GENERATED LOGGING
    // ---------------------------------------------------------
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning,
        Message = "PerformanceBucketService received null key.")]
    private partial void LogNullKey();

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug,
        Message = "Bucket found → {BucketKey} (Samples={SampleCount})")]
    private partial void LogBucketFound(string bucketKey, int sampleCount);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug,
        Message = "Bucket missing → {BucketKey}")]
    private partial void LogBucketMissing(string bucketKey);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information,
        Message = "Created new performance bucket → {BucketKey}")]
    private partial void LogBucketCreated(string bucketKey);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug,
        Message = "Recorded sample → {BucketKey} Input={InputSeconds}s Encode={EncodeSeconds}s (TotalSamples={SampleCount})")]
    private partial void LogSampleRecorded(string bucketKey, double InputSeconds, double EncodeSeconds, int SampleCount);

    [LoggerMessage(EventId = 6, Level = LogLevel.Debug,
        Message = "Estimated encode speed for {BucketKey} → {SecondsPerSecond} sec/sec")]
    private partial void LogSpeedEstimated(string bucketKey, double SecondsPerSecond);

    [LoggerMessage(EventId = 7, Level = LogLevel.Warning,
        Message = "Encode speed fallback used for {BucketKey}")]
    private partial void LogSpeedFallback(string bucketKey);

    [LoggerMessage(EventId = 8, Level = LogLevel.Debug,
        Message = "Estimated load for {BucketKey} → {LoadPercent}%")]
    private partial void LogLoadEstimated(string bucketKey, int LoadPercent);

    [LoggerMessage(EventId = 9, Level = LogLevel.Warning,
        Message = "Load fallback used for {BucketKey}")]
    private partial void LogLoadFallback(string bucketKey);
}
