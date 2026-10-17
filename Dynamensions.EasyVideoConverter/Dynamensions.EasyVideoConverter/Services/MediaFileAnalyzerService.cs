using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Models.Ffprobe;
using Dynamensions.EasyVideoConverter.Core.Serialization;
using Dynamensions.EasyVideoConverter.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamensions.EasyVideoConverter.Services;

public sealed partial class MediaFileAnalyzerService(ILogger<MediaFileAnalyzerService> logger, ISettingsService settingsService) : IMediaFileAnalyzerService
{
    // ---------------------------------------------------------
    // PUBLIC ENTRY POINT
    // ---------------------------------------------------------
    public async Task<MediaInfo> AnalyzeAsync(string filePath, CancellationToken cancellationToken)
    {
        LogAnalyzeStart(filePath);

        string json = await RunFfprobeAsync(filePath, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            LogEmptyJson(filePath);
            return new MediaInfo(); // fail-safe
        }

        FfprobeRoot? root;
        try
        {
            root = JsonSerializer.Deserialize<FfprobeRoot>(
                json,
                EncoderProfileJsonContext.Default.FfprobeRoot
            );
        }
        catch (Exception ex)
        {
            LogJsonDeserializeFailed(filePath);
            logger.LogError(ex, "Failed to deserialize ffprobe JSON for {FilePath}", filePath);
            return new MediaInfo();
        }

        if (root == null)
        {
            LogNullRoot(filePath);
            return new MediaInfo();
        }

        var info = BuildMediaInfo(root);
        LogAnalyzeSuccess(filePath);
        return info;
    }

    // ---------------------------------------------------------
    // RUN FFPROBE
    // ---------------------------------------------------------
    private async Task<string> RunFfprobeAsync(string filePath, CancellationToken cancellationToken)
    {
        var args = $"-v quiet -print_format json -show_streams -show_format \"{filePath}\"";
        StringBuilder error = new();
        StringBuilder output = new();

        // ---------------------------------------------------------
        // EXECUTE FFProbe
        // ---------------------------------------------------------
        string ffProbePath = Path.Combine(settingsService.FfMpegDirectory, "ffprobe.exe");


        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffProbePath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        LogFfprobeStart(ffProbePath, args);



        using var registration = cancellationToken.Register(() =>
        {
            LogCancellationRequested(filePath);
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
                LogKillFailed(logger, filePath, ex.Message);
            }
        });

        process.ErrorDataReceived += (s, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                error.AppendLine(e.Data);
                LogFfprobeErrorLine(e.Data);
            }
        };

        process.OutputDataReceived += (s, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                output.AppendLine(e.Data);
                LogFfprobeOutputLine(e.Data);
            }
        };

        try
        {
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            await process.WaitForExitAsync(cancellationToken);
            LogFfprobeExitCode(filePath, process.ExitCode);
        }
        catch (TaskCanceledException)
        {
            LogFfprobeCancelled(filePath);
        }
        catch (Exception ex)
        {
            LogFfprobeFailed(filePath);
            logger.LogError(ex, "ffprobe failed for {FilePath}", filePath);
        }

        if (error.Length > 0 && logger.IsEnabled(LogLevel.Debug))
        {
            string stderr = error.ToString();
            LogFfprobeStderr(logger, filePath, stderr);
        }

        return output.ToString();
    }

    // ---------------------------------------------------------
    // BUILD MEDIAINFO MODEL
    // ---------------------------------------------------------
    private static MediaInfo BuildMediaInfo(FfprobeRoot root)
    {
        var info = new MediaInfo();

        var video = root.Streams.FirstOrDefault(s => s.CodecType == "video");
        var audioTracks = root.Streams.Where(s => s.CodecType == "audio").ToList();
        var subtitleTracks = root.Streams.Where(s => s.CodecType == "subtitle").ToList();

        // CONTAINER
        info.Container = root.Format?.FormatName?.Split(',').FirstOrDefault() ?? "unknown";

        // VIDEO
        info.VideoCodec = video?.CodecName ?? "unknown";

        info.Width = video?.Width ?? 0;
        info.Height = video?.Height ?? 0;

        info.CodedWidth = video?.CodedWidth ?? info.Width;
        info.CodedHeight = video?.CodedHeight ?? info.Height;

        info.FrameRate = ParseFrameRate(video?.AvgFrameRate);
        info.BitRate = ParseLong(root.Format?.BitRate);
        info.Duration = ParseDuration(root.Format?.Duration);

        info.PixelFormat = video?.PixFmt ?? "unknown";

        if (!string.IsNullOrWhiteSpace(info.PixelFormat))
        {
            if (info.PixelFormat.Contains("12"))
                info.BitDepth = 12;
            else if (info.PixelFormat.Contains("10"))
                info.BitDepth = 10;
            else
                info.BitDepth = 8;
        }

        info.Is10Bit = info.BitDepth > 8;

        info.IsHdr =
            video?.ColorTransfer == "smpte2084" ||
            video?.ColorTransfer == "arib-std-b67";

        info.H264Profile = video?.Profile;

        if (video?.Level != null && video.Level > 0)
            info.H264Level = video.Level / 10.0;

        // Dolby Vision
        if (video?.SideDataList?.Any(sd =>
                sd.SideDataType.Contains("Dolby Vision", StringComparison.OrdinalIgnoreCase)) == true)
        {
            info.IsDolbyVision = true;
        }

        // HDR10+
        if (video?.SideDataList?.Any(sd =>
                sd.SideDataType.Contains("HDR10+", StringComparison.OrdinalIgnoreCase)) == true)
        {
            info.IsHdr10Plus = true;
        }

        bool isPQ = video?.ColorTransfer?.Equals("smpte2084", StringComparison.OrdinalIgnoreCase) == true;
        bool isBT2020 = video?.ColorPrimaries?.Equals("bt2020", StringComparison.OrdinalIgnoreCase) == true;

        if (!info.IsDolbyVision && !info.IsHdr10Plus && isPQ && isBT2020)
        {
            info.IsHdr10 = true;
        }

        // PAR / SAR
        var sar = video?.SampleAspectRatio;

        info.SarNum = 1;
        info.SarDen = 1;
        info.PixelAspectRatio = 1.0;

        if (!string.IsNullOrWhiteSpace(sar) && sar.Contains(':'))
        {
            var parts = sar.Split(':');
            if (int.TryParse(parts[0], out int num) &&
                int.TryParse(parts[1], out int den) &&
                num > 0 && den > 0)
            {
                info.SarNum = num;
                info.SarDen = den;
                info.PixelAspectRatio = num / (double)den;
            }
        }

        // AUDIO TRACKS
        info.AudioTracks = [.. audioTracks.Select((a, i) => new AudioTrack
        {
            Index = a.Index,
            FfmpegIndex = i,
            Codec = a.CodecName ?? "unknown",
            IsAacLc = (a.CodecName == "aac" &&
                       (a.Profile?.Equals("LC", StringComparison.OrdinalIgnoreCase) ?? false)),
            
            // bitrate may be on the tag.
            BitRate = a.BitRate != null ? ParseLong(a.BitRate) : ParseLong(a.Tags.BPS),
            Channels = a.Channels,
            Language = a.Tags?.Language ?? "und",
            Title = a.Tags?.Title
        })];

        info.PrimaryAudio = info.AudioTracks.FirstOrDefault();

        // SUBTITLE TRACKS
        info.Subtitles = [.. subtitleTracks.Select((s, i) => new SubtitleTrack
        {
            Index = s.Index,
            FfmpegIndex = i,
            Codec = s.CodecName ?? "unknown",
            Language = s.Tags?.Language ?? "und",
            Title = s.Tags?.Title
        })];

        info.HasSubtitles = info.Subtitles.Count != 0;

        info.HasSrtSubtitles = info.Subtitles.Any(s =>
            s.Codec.Equals("subrip", StringComparison.OrdinalIgnoreCase) ||
            s.Codec.Equals("srt", StringComparison.OrdinalIgnoreCase));

        info.HasPgsSubtitles = info.Subtitles.Any(s =>
            s.Codec.Equals("hdmv_pgs_subtitle", StringComparison.OrdinalIgnoreCase));

        info.HasMovTextSubtitles = info.Subtitles.Any(s =>
            s.Codec.Equals("mov_text", StringComparison.OrdinalIgnoreCase));

        return info;
    }

    // ---------------------------------------------------------
    // SAFE PARSERS
    // ---------------------------------------------------------
    private static TimeSpan ParseDuration(string? value)
    {
        if (double.TryParse(value, out double seconds))
            return TimeSpan.FromSeconds(seconds);

        return TimeSpan.Zero;
    }

    private static long ParseLong(string? value)
    {
        if (long.TryParse(value, out long result))
            return result;

        return 0;
    }

    private static double ParseFrameRate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        if (value.Contains('/'))
        {
            var parts = value.Split('/');
            if (double.TryParse(parts[0], out double num) &&
                double.TryParse(parts[1], out double den) &&
                den != 0)
            {
                return num / den;
            }
            return 0;
        }

        if (double.TryParse(value, out double fr))
            return fr;

        return 0;
    }

    // ---------------------------------------------------------
    // SOURCE-GENERATED LOGGING
    // ---------------------------------------------------------
    [LoggerMessage(EventId = 1, Level = LogLevel.Information,
        Message = "Analyzing media file → {FilePath}")]
    private partial void LogAnalyzeStart(string filePath);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning,
        Message = "ffprobe returned empty JSON for {FilePath}.")]
    private partial void LogEmptyJson(string filePath);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error,
        Message = "Failed to deserialize ffprobe JSON for {FilePath}.")]
    private partial void LogJsonDeserializeFailed(string filePath);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning,
        Message = "ffprobe root object was null for {FilePath}.")]
    private partial void LogNullRoot(string filePath);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information,
        Message = "Media analysis completed successfully for {FilePath}.")]
    private partial void LogAnalyzeSuccess(string filePath);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information,
        Message = "Starting ffprobe → {Executable} {Arguments}")]
    private partial void LogFfprobeStart(string executable, string arguments);

    [LoggerMessage(EventId = 7, Level = LogLevel.Warning,
        Message = "Cancellation requested, stopping ffprobe for {FilePath}.")]
    private partial void LogCancellationRequested(string filePath);

    [LoggerMessage(EventId = 8, Level = LogLevel.Debug,
        Message = "ffprobe stderr: {Line}")]
    private partial void LogFfprobeErrorLine(string line);

    [LoggerMessage(EventId = 9, Level = LogLevel.Debug,
        Message = "ffprobe stdout: {Line}")]
    private partial void LogFfprobeOutputLine(string line);

    [LoggerMessage(EventId = 10, Level = LogLevel.Warning,
        Message = "ffprobe cancelled for {FilePath}.")]
    private partial void LogFfprobeCancelled(string filePath);

    [LoggerMessage(EventId = 11, Level = LogLevel.Error,
        Message = "ffprobe failed for {FilePath}.")]
    private partial void LogFfprobeFailed(string filePath);

    [LoggerMessage(EventId = 12, Level = LogLevel.Information,
        Message = "ffprobe exited for {FilePath} with code {ExitCode}.")]
    private partial void LogFfprobeExitCode(string filePath, int exitCode);

    [LoggerMessage(EventId = 20, Level = LogLevel.Debug,
    Message = "Failed to kill ffprobe process for {FilePath}. Exception: {Message}")]
    private static partial void LogKillFailed(ILogger logger, string filePath, string message);

    [LoggerMessage(EventId = 21, Level = LogLevel.Debug,
        Message = "ffprobe stderr for {FilePath}: {Error}")]
    private static partial void LogFfprobeStderr(ILogger logger, string filePath, string error);

}
