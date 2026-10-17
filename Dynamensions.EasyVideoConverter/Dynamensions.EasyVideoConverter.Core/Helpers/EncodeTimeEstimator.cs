using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Models.Enums;
using Dynamensions.EasyVideoConverter.Core.Services;

namespace Dynamensions.EasyVideoConverter.Core.Helpers;

public sealed class EncoderCapabilities
{
    public bool HasCpu { get; init; } = true;
    public bool HasNvidia { get; init; }
    public bool HasIntelQsv { get; init; }
    public bool HasAmdAmf { get; init; }
}


public class EncodeTimeEstimator(IPerformanceBucketService performanceBucketService)
{

    // ---------------------------------------------------------
    // RECORD SAMPLE
    // ---------------------------------------------------------
    public void RecordJob(PerformanceBucketKey key, double sourceSeconds, double encodeSeconds)
    {
        performanceBucketService.RecordJob(key, sourceSeconds, encodeSeconds);
    }

    // ---------------------------------------------------------
    // PRE-ENCODE ESTIMATE
    // ---------------------------------------------------------
    public double EstimatePreEncode(PerformanceBucketKey key, double sourceSeconds, int activeJobs = 1)
    {
        var bucket = performanceBucketService.GetBucket(key);

        double secondsPerSecond;

        // ---------------------------------------------------------
        // BASELINE FPS (fallback)
        // ---------------------------------------------------------
        if (bucket == null || bucket.SampleCount == 0)
        {
            // Realistic baseline FPS for your pipeline
            double baselineFps = key.EncoderType switch
            {
                "NVIDIA" => 55.0,   // empirically validated
                "QSV" => 50.0,   // tuned relative to NVENC
                "AMF" => 40.0,   // tuned relative to NVENC
                _ => 12.0    // CPU fallback
            };

            secondsPerSecond = 1.0 / baselineFps;

            // Cold-start penalty (hardware spin-up, filtergraph init)
            secondsPerSecond *= 1.20;
        }
        else
        {
            secondsPerSecond = bucket.SecondsPerSecond;
        }

        // ---------------------------------------------------------
        // RESOLUTION SCALING
        // ---------------------------------------------------------
        double megapixels = (key.Width * key.Height) / 1_000_000.0;

        // Scaling cost grows non-linearly
        secondsPerSecond *= megapixels switch
        {
            <= 1.0 => 1.00,  // 720p
            <= 2.0 => 1.15,  // 1080p
            <= 3.5 => 1.35,  // 1440p
            <= 8.5 => 1.65,  // 4K
            _ => 2.00        // >4K
        };

        // ---------------------------------------------------------
        // PIX_FMT CONVERSION COST
        // ---------------------------------------------------------
        if (key.BitDepth == 10)
            secondsPerSecond *= 1.20; // 10-bit → more bandwidth + more memory ops

        // ---------------------------------------------------------
        // BURN-IN COST
        // ---------------------------------------------------------
        if (key.BurnIn)
        {
            secondsPerSecond *= key.EncoderType switch
            {
                "NVIDIA" => 1.30,   // NVENC handles burn-in well but still costs
                "QSV" => 1.40,
                "AMF" => 1.50,
                _ => 1.80    // CPU burn-in is expensive
            };
        }

        // ---------------------------------------------------------
        // CODEC COMPLEXITY COST
        // ---------------------------------------------------------
        secondsPerSecond *= key.OutputCodec switch
        {
            "h264" => 1.00,
            "hevc" => 1.20,
            "av1" => 1.50,
            _ => 1.00
        };

        // ---------------------------------------------------------
        // MULTI-JOB LOAD SCALING
        // ---------------------------------------------------------
        if (activeJobs > 1)
        {
            double loadPenalty = 1 + (activeJobs - 1) * 0.25;
            secondsPerSecond *= loadPenalty;
        }

        // ---------------------------------------------------------
        // FINAL ESTIMATE
        // ---------------------------------------------------------
        return sourceSeconds * secondsPerSecond;
    }


    // ---------------------------------------------------------
    // LIVE ESTIMATE
    // ---------------------------------------------------------
    public static double EstimateLive(double encodedSeconds, double totalSeconds, double elapsedSeconds)
    {
        double progress = encodedSeconds / totalSeconds;

        if (progress <= 0.01)
            return totalSeconds * 1.5;

        return elapsedSeconds * (1 / progress - 1);
    }

    public static PerformanceBucketKey CreatePerformanceKey(MediaInfo mediaInfo, SmartDecision smartDecision)
    {
        return new PerformanceBucketKey
        {
            InputCodec = mediaInfo.VideoCodec,
            OutputCodec = smartDecision.SelectedVideoCodec,

            Width = smartDecision.TargetWidth,
            Height = smartDecision.TargetHeight,

            BitDepth = smartDecision.TargetBitDepth,

            // CPU / NVIDIA / QSV / AMF
            EncoderType = smartDecision.EncoderType,

            BurnIn = smartDecision.SubtitleHandling == SubtitleMode.BurnIn
        };
    }


}
