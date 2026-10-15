using Dynamensions.EasyVideoConverter.Core.Helpers;
using Dynamensions.EasyVideoConverter.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace Dynamensions.EasyVideoConverter.Services;

internal class SystemService(ILogger<SystemService> logger) : ISystemService
{
    private readonly ILogger<SystemService> _logger = logger;

    public EncoderCapabilities GetEncoderCapabilities()
    {
        SmartDecisionSystemLog.GpuScanBegin(_logger);

        var gpus = GetGpuNames();

        foreach (var gpu in gpus)
        {
            SmartDecisionSystemLog.GpuFound(_logger, gpu);
        }

        bool hasNvidia = gpus.Any(n => n.Contains("nvidia", StringComparison.OrdinalIgnoreCase));
        bool hasIntel = gpus.Any(n => n.Contains("intel", StringComparison.OrdinalIgnoreCase));
        bool hasAmd = gpus.Any(n => n.Contains("amd", StringComparison.OrdinalIgnoreCase)) ||
                         gpus.Any(n => n.Contains("radeon", StringComparison.OrdinalIgnoreCase));

        SmartDecisionSystemLog.GpuCapabilitiesDetected(
            _logger,
            hasNvidia,
            hasIntel,
            hasAmd
        );

        return new EncoderCapabilities
        {
            HasCpu = true,
            HasNvidia = hasNvidia,
            HasIntelQsv = hasIntel,
            HasAmdAmf = hasAmd
        };
    }

    public bool HasFfmpegEncoder(string ffmpegPath, string encoderName)
    {
        SmartDecisionSystemLog.FfmpegEncoderCheckBegin(_logger, encoderName);

        var psi = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = "-hide_banner -encoders",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi);
        string output = proc!.StandardOutput.ReadToEnd();
        proc.WaitForExit();

        bool found = output.Contains(encoderName, StringComparison.OrdinalIgnoreCase);

        SmartDecisionSystemLog.FfmpegEncoderCheckResult(_logger, encoderName, found);

        return found;
    }

    private static string[] GetGpuNames()
    {
        using var searcher = new ManagementObjectSearcher("select Name from Win32_VideoController");
        return [.. searcher.Get()
                       .Cast<ManagementObject>()
                       .Select(mo => (mo["Name"]?.ToString() ?? string.Empty))];
    }
}

internal static partial class SmartDecisionSystemLog
{
    // ---------------------------------------------------------
    // 2800–2899 — GPU DETECTION
    // ---------------------------------------------------------

    [LoggerMessage(EventId = 2800, Level = LogLevel.Information, Message = "Scanning system GPUs.")]
    public static partial void GpuScanBegin(ILogger logger);

    [LoggerMessage(EventId = 2801, Level = LogLevel.Information, Message = "GPU detected: {GpuName}")]
    public static partial void GpuFound(ILogger logger, string GpuName);

    [LoggerMessage(
        EventId = 2802,
        Level = LogLevel.Information,
        Message = "GPU capabilities → NVIDIA={HasNvidia}, Intel={HasIntel}, AMD={HasAmd}"
    )]
    public static partial void GpuCapabilitiesDetected(
        ILogger logger,
        bool HasNvidia,
        bool HasIntel,
        bool HasAmd
    );

    // ---------------------------------------------------------
    // 2900–2999 — FFMPEG ENCODER DETECTION
    // ---------------------------------------------------------

    [LoggerMessage(EventId = 2900, Level = LogLevel.Information, Message = "Checking FFmpeg encoder: {EncoderName}")]
    public static partial void FfmpegEncoderCheckBegin(ILogger logger, string EncoderName);

    [LoggerMessage(EventId = 2901, Level = LogLevel.Information, Message = "FFmpeg encoder {EncoderName} found={Found}")]
    public static partial void FfmpegEncoderCheckResult(ILogger logger, string EncoderName, bool Found);
}
