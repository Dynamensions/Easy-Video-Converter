using Dynamensions.EasyVideoConverter.Core.Helpers;
using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamensions.EasyVideoConverter.Services;

public sealed partial class SubtitleExtractorService(ILogger<SubtitleExtractorService> logger, ISettingsService settingsService) : ISubtitleExtractorService
{
    // ---------------------------------------------------------
    // 1. Extract subtitle for burn-in
    // ---------------------------------------------------------
    public async Task<string?> ExtractForBurnInAsync(
        QueueItem item,
        Action<(QueueItem item, double ProgressValue, double CurrentFPS, double speedX, double secondsRemaining)>? progressCallback,
        CancellationToken cancellationToken)
    {
        LogExtractionMode(logger, "Burn-In");

        var sub = item.Task.Decision.SelectedSubtitle;
        if (sub == null)
            return null;

        var baseName = Path.GetFileNameWithoutExtension(item.Task.File.FileName);
        var tempPath = Path.Combine(Path.GetTempPath(), $"{baseName}_burnin.srt");

        var codec = sub.Codec switch
        {
            "subrip" => "srt",
            "ass" => "ass",
            "mov_text" => "srt",
            "hdmv_pgs_subtitle" => "srt",
            _ => "srt"
        };

        LogCodecSelection(logger, codec, sub.FfmpegIndex);

        if (sub.Codec == "hdmv_pgs_subtitle")
        {
            var supPath = Path.Combine(Path.GetTempPath(), $"{baseName}_burnin.sup");
            var argsSup = $"-i \"{item.Task.File.Path}\" -map 0:s:{sub.FfmpegIndex} \"{supPath}\"";

            await RunFFmpegAsync(argsSup, item, progressCallback, cancellationToken);

            if (!File.Exists(supPath))
            {
                LogPgsFailed(logger, supPath);
                return null;
            }

            File.WriteAllText(tempPath,
                "1\n00:00:00,000 --> 00:00:01,000\n[OCR placeholder: PGS requires external OCR]");
        }
        else
        {
            var args = $"-i \"{item.Task.File.Path}\" -map 0:s:{sub.FfmpegIndex} -c:s {codec} \"{tempPath}\"";
            await RunFFmpegAsync(args, item, progressCallback, cancellationToken);
        }

        ValidateExtraction(tempPath);
        return tempPath;
    }

    // ---------------------------------------------------------
    // 2. Extract external subtitles
    // ---------------------------------------------------------
    public async Task ExportAllAsync(
        QueueItem item,
        IEnumerable<SubtitleTrack> tracks,
        Action<(QueueItem item, double ProgressValue, double CurrentFPS, double speedX, double secondsRemaining)>? progressCallback,
        Action<(QueueItem item, int CurrentTrack, int TotalTracks, SubtitleTrack subtitleTrack)>? trackCallback,
        CancellationToken cancellationToken)
    {
        LogExtractionMode(logger, "External (Batch)");

        var baseName = Path.GetFileNameWithoutExtension(item.Task.File.FileName);
        var folder = Path.GetDirectoryName(item.Task.OutputPath)!;

        LogOutputFolder(logger, folder);

        var totalTracks = tracks.Count();
        var current = 1;

        foreach (var sub in tracks)
        {
            trackCallback?.Invoke((item, current++, totalTracks, sub));

            var output = Path.Combine(folder, $"{baseName}.{sub.Language}.srt");

            var codec = sub.Codec switch
            {
                "subrip" => "srt",
                "ass" => "ass",
                "mov_text" => "srt",
                _ => "srt"
            };

            LogTrackMetadata(logger, sub.Language, codec, sub.FfmpegIndex, output);
            LogCodecSelection(logger, codec, sub.FfmpegIndex);

            var args = $"-i \"{item.Task.File.Path}\" -map 0:s:{sub.FfmpegIndex} -c:s {codec} \"{output}\"";

            await RunFFmpegAsync(args, item, progressCallback, cancellationToken);

            if (!File.Exists(output) || new FileInfo(output).Length == 0)
                LogExternalSubtitleFailed(logger, output);
            else
                LogExternalSubtitleCreated(logger, output);
        }
    }

    public async Task ExportAsync(
        QueueItem item,
        SubtitleTrack track,
        Action<(QueueItem item, double ProgressValue, double CurrentFPS, double speedX, double secondsRemaining)>? progressCallback,
        CancellationToken cancellationToken)
    {
        LogExtractionMode(logger, "External (Single)");

        var baseName = Path.GetFileNameWithoutExtension(item.Task.File.FileName);
        var folder = Path.GetDirectoryName(item.Task.OutputPath)!;

        LogOutputFolder(logger, folder);

        var output = Path.Combine(folder, $"{baseName}.{track.Language}.srt");

        var codec = track.Codec switch
        {
            "subrip" => "srt",
            "ass" => "ass",
            "mov_text" => "srt",
            _ => "srt"
        };

        LogTrackMetadata(logger, track.Language, codec, track.FfmpegIndex, output);
        LogCodecSelection(logger, codec, track.FfmpegIndex);

        var args = $"-i \"{item.Task.File.Path}\" -map 0:s:{track.FfmpegIndex} -c:s {codec} \"{output}\"";

        await RunFFmpegAsync(args, item, progressCallback, cancellationToken);

        if (!File.Exists(output) || new FileInfo(output).Length == 0)
            LogExternalSubtitleFailed(logger, output);
        else
            LogExternalSubtitleCreated(logger, output);
    }

    // ---------------------------------------------------------
    // 3. FFmpeg runner
    // ---------------------------------------------------------
    private async Task<int> RunFFmpegAsync(
        string args,
        QueueItem item,
        Action<(QueueItem item, double ProgressValue, double CurrentFPS, double speedX, double secondsRemaining)>? progressCallback, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var currentFPS = 0;
        var currentSpeedX = 0.0;

        args = "-y " + args;

        string ffmpegPath = Path.Combine(settingsService.FfMpegDirectory, "ffmpeg.exe");


        var psi = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        LogFfmpegStart(logger, psi.FileName, psi.Arguments);

        using var process = new Process { StartInfo = psi };


        process.ErrorDataReceived += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(e.Data))
                return;

            //LogFfmpegDebug(logger, e.Data);

            if (!e.Data.Contains("time="))
            {
                LogProgressParseMiss(logger, e.Data);
                return;
            }

            var match = TimeRegex().Match(e.Data);
            if (match.Success)
            {
                var parsedTime = TimeSpan.Parse(match.Groups[1].Value);
                var percent = parsedTime.TotalSeconds / item.Task.Info.Duration.TotalSeconds;

                var secondsRemaining = EncodeTimeEstimator.EstimateLive(
                    parsedTime.TotalSeconds,
                    item.Task.Info.Duration.TotalSeconds,
                    stopwatch.Elapsed.TotalSeconds);

                progressCallback?.Invoke((item, percent, currentFPS, currentSpeedX, secondsRemaining));
            }


            var fpsMatch = FPSRegex().Match(e.Data);
            if (fpsMatch.Success)
                currentFPS = int.Parse(fpsMatch.Groups[1].Value);

            var speedMatch = SpeedRegex().Match(e.Data);
            if (speedMatch.Success)
                currentSpeedX = double.Parse(speedMatch.Groups[1].Value);
        };

        LogCancellationRegistered(logger);

        using var registration = cancellationToken.Register(() =>
        {
            try
            {
                if (!process.HasExited && process.Id != 0)
                {
                    logger.LogInformation("Cancellation requested, stopping process...");
                    process.Kill(entireProcessTree: true);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to kill ffprobe process for {FilePath}. {file}", item.Task.OutputPath, item.Task.OutputFileName);
            }
        });


        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        try
        {
            await process.WaitForExitAsync(item.Cancellation.Token);
        }
        catch (TaskCanceledException)
        {
            LogSubtitleCancelled(logger);
        }

        LogFfmpegExitCode(logger, process.ExitCode);
        return process.ExitCode;
    }

    // ---------------------------------------------------------
    // 4. Validate extraction
    // ---------------------------------------------------------
    private void ValidateExtraction(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Subtitle extraction failed: {path}");

        if (new FileInfo(path).Length == 0)
            throw new InvalidDataException($"Subtitle extraction produced empty file: {path}");

        LogSubtitleExtracted(logger, path);
    }

    // ---------------------------------------------------------
    // 5. Regex
    // ---------------------------------------------------------
    [GeneratedRegex(@"time=(\d+:\d+:\d+.\d+)")]
    private static partial Regex TimeRegex();

    [GeneratedRegex(@"fps=\s*(\d+)")]
    private static partial Regex FPSRegex();

    [GeneratedRegex(@"speed=\s*([\d\.]+)x")]
    private static partial Regex SpeedRegex();

    // ---------------------------------------------------------
    // 6. Source-generated logging
    // ---------------------------------------------------------
    [LoggerMessage(EventId = 1, Level = LogLevel.Information,
        Message = "External subtitle created → {Path}")]
    private static partial void LogExternalSubtitleCreated(ILogger logger, string path);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning,
        Message = "External subtitle extraction failed → {Path}")]
    private static partial void LogExternalSubtitleFailed(ILogger logger, string path);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning,
        Message = "Cancellation requested, stopping FFmpeg...")]
    private static partial void LogCancellationRequested(ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning,
        Message = "Subtitle extraction cancelled.")]
    private static partial void LogSubtitleCancelled(ILogger logger);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug,
        Message = "[FFmpeg] {Line}")]
    private static partial void LogFfmpegDebug(ILogger logger, string line);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information,
        Message = "Subtitle extracted successfully → {Path}")]
    private static partial void LogSubtitleExtracted(ILogger logger, string path);

    [LoggerMessage(EventId = 7, Level = LogLevel.Error,
        Message = "PGS extraction failed → {Path}")]
    private static partial void LogPgsFailed(ILogger logger, string path);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information,
        Message = "Starting FFmpeg → {Executable} {Arguments}")]
    private static partial void LogFfmpegStart(ILogger logger, string executable, string arguments);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information,
        Message = "FFmpeg exited with code {ExitCode}")]
    private static partial void LogFfmpegExitCode(ILogger logger, int exitCode);

    [LoggerMessage(EventId = 10, Level = LogLevel.Debug,
        Message = "Extracting subtitle track → Lang={Lang}, Codec={Codec}, Index={Index}, Output={Output}")]
    private static partial void LogTrackMetadata(ILogger logger, string lang, string codec, int index, string output);

    [LoggerMessage(EventId = 11, Level = LogLevel.Debug,
        Message = "FFmpeg progress line did not match expected format → {Line}")]
    private static partial void LogProgressParseMiss(ILogger logger, string line);

    [LoggerMessage(EventId = 12, Level = LogLevel.Debug,
        Message = "Registered cancellation callback for FFmpeg process.")]
    private static partial void LogCancellationRegistered(ILogger logger);

    [LoggerMessage(EventId = 13, Level = LogLevel.Information,
        Message = "Subtitle extraction mode → {Mode}")]
    private static partial void LogExtractionMode(ILogger logger, string mode);

    [LoggerMessage(EventId = 14, Level = LogLevel.Debug,
        Message = "Selected subtitle codec → {Codec} for track index {Index}")]
    private static partial void LogCodecSelection(ILogger logger, string codec, int index);

    [LoggerMessage(EventId = 15, Level = LogLevel.Debug,
        Message = "Ensured output folder exists → {Folder}")]
    private static partial void LogOutputFolder(ILogger logger, string folder);
}
