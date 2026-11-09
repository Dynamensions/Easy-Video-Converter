using System.Collections.Generic;
using System.Linq;

namespace Dynamensions.EasyVideoConverter.Core.Models;

public class PerformanceBucket
{
    private readonly List<PerformanceSample> _samples = [];

    public int SampleCount => _samples.Count;

    // Average encode speed: encodeSeconds / inputSeconds
    public double SecondsPerSecond =>
        _samples.Count == 0
            ? 0.95
            : _samples.Average(s => s.EncodeSeconds / s.InputSeconds);

    // Estimated load percent (0–100)
    public int EstimatedLoadPercent =>
        (int)(SecondsPerSecond * 100);

    public void AddSample(double inputSeconds, double encodeSeconds)
    {
        _samples.Add(new PerformanceSample
        {
            InputSeconds = inputSeconds,
            EncodeSeconds = encodeSeconds
        });
    }
}

public class PerformanceSample
{
    public double InputSeconds { get; set; }
    public double EncodeSeconds { get; set; }
}