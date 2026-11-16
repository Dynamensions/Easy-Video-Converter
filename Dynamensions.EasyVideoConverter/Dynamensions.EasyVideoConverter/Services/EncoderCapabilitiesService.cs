using Dynamensions.EasyVideoConverter.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamensions.EasyVideoConverter.Services;

public partial class EncoderCapabilitiesService(ILogger<EncoderCapabilitiesService> logger) : IEncoderCapabilitiesService
{
    public EncoderCapabilities EncoderCapabilities { get; private set; } = new();

    public async Task<EncoderCapabilities> GetEncoderCapabilitiesAsync(IProgress<(double Percent, string Status)> progress, CancellationToken cancellationToken)
    {
        var total = 3.0;
        var current = 1.0;

        progress.Report((current++ / total, "Testing Encoder Capabilities: NvEnc"));
        var (Output, Error, ExitCode) = await ProbeEncoderAsync("h264_nvenc", cancellationToken);
        logger.LogInformation("NvEnc Probe Output: {Output}, Error: {Error}, ExitCode: {ExitCode}", Output, Error, ExitCode);


        progress.Report((current++ / total, "Testing Encoder Capabilities: Intel QSV"));
        var (qsvOutput, qsvError, qsvExitCode) = await ProbeEncoderAsync("h264_qsv", cancellationToken);
        logger.LogInformation("Intel QSV Probe Output: {Output}, Error: {Error}, ExitCode: {ExitCode}", qsvOutput, qsvError, qsvExitCode);


        progress.Report((current++ / total, "Testing Encoder Capabilities: AMD AMF"));
        var (amfOutput, amfError, amfExitCode) = await ProbeEncoderAsync("h264_amf", cancellationToken);
        logger.LogInformation("AMD AMF Probe Output: {Output}, Error: {Error}, ExitCode: {ExitCode}", amfOutput, amfError, amfExitCode);

        progress.Report((total, "Encoder requirements complete."));
        EncoderCapabilities.HasCpu = true;
        EncoderCapabilities.HasNvidia = ExitCode == 0;
        EncoderCapabilities.HasIntelQsv = qsvExitCode == 0;
        EncoderCapabilities.HasAmdAmf = amfExitCode == 0;

        logger.LogInformation("Encoder Capabilities: HasCpu={HasCpu}, HasNvidia={HasNvidia}, HasIntelQsv={HasIntelQsv}, HasAmdAmf={HasAmdAmf}",
            EncoderCapabilities.HasCpu,
            EncoderCapabilities.HasNvidia,
            EncoderCapabilities.HasIntelQsv,
            EncoderCapabilities.HasAmdAmf);

        return EncoderCapabilities;
    }

    private async Task<(string Output, string Error, int ExitCode)> ProbeEncoderAsync(string arg, CancellationToken cancellationToken)
    {
        var output = await RunFfmpegAsync(
            Path.Combine(App.FfMpegDirectory, "ffmpeg.exe"),
            $"-hide_banner -f lavfi -i testsrc=size=1920x1080:rate=1 -frames:v 1 -c:v {arg} -f null -",
            cancellationToken);

        return output;
    }

    private async Task<(string Output, string Error, int ExitCode)> RunFfmpegAsync(
        string ffmpegPath,
        string arguments,
        CancellationToken cancellationToken = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start FFmpeg.");

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        return (stdout, stderr, process.ExitCode);
    }


    [LoggerMessage(EventId = 4, Level = LogLevel.Warning,
    Message = "Encoder detection cancelled.")]
    private static partial void LogEncoderDetectionCancelled(ILogger logger);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information,
      Message = "Starting FFmpeg → {Executable} {Arguments}")]
    private static partial void LogFfmpegStart(ILogger logger, string executable, string arguments);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information,
        Message = "FFmpeg exited with code {ExitCode}")]
    private static partial void LogFfmpegExitCode(ILogger logger, int exitCode);

    [LoggerMessage(EventId = 11, Level = LogLevel.Debug,
       Message = "FFmpeg progress line did not match expected format → {Line}")]
    private static partial void LogProgressParseMiss(ILogger logger, string line);

    [LoggerMessage(EventId = 12, Level = LogLevel.Debug,
    Message = "Registered cancellation callback for FFmpeg process.")]
    private static partial void LogCancellationRegistered(ILogger logger);
}