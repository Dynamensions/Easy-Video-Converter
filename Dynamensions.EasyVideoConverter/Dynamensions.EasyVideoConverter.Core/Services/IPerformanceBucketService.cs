using Dynamensions.EasyVideoConverter.Core.Models;

namespace Dynamensions.EasyVideoConverter.Core.Services;

public interface IPerformanceBucketService
{
    PerformanceBucket? GetBucket(PerformanceBucketKey key);
    void RecordJob(PerformanceBucketKey key, double inputDurationSeconds, double encodeDurationSeconds);
    double EstimateSecondsPerSecond(PerformanceBucketKey key);
    int EstimateLoadPercent(PerformanceBucketKey key);
}
