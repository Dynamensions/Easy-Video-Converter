using Dynamensions.EasyVideoConverter.Core.Helpers;
using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Models.Enums;
using Dynamensions.EasyVideoConverter.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dynamensions.EasyVideoConverter.Services;

public partial class TranscodeService(ILogger<TranscodeService> logger, EncodeTimeEstimator encodeTimeEstimator, ISettingsService settingsService) : ITranscodeService
{

    public async Task TranscodeAsync(QueueItem queueItem, Action<(QueueItem item, double ProgressValue, double CurrentFPS, double speedX, double secondsRemaining)>? progressCallback)
    {

        var stopwatch = Stopwatch.StartNew();
        var currentFPS = 0;
        var currentSpeedX = 0.0;
        var d = queueItem.Task.Decision;
        var task = queueItem.Task;
        var burnSubPath = task.BurnInSubtitlePath;


        // ---------------------------------------------------------
        // BUILD FINAL COMMAND
        // ---------------------------------------------------------
        var args = Build(task, d, burnSubPath);

        logger.LogInformation("Starting FFmpeg process for {File}", task.File.Path);

        // ---------------------------------------------------------
        // EXECUTE FFMPEG
        // ---------------------------------------------------------
        string? ffmpegPath = Path.Combine(settingsService.FfMpegDirectory, "ffmpeg.exe");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = args,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }

        };

        process.OutputDataReceived += (s, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {

            }
        };


        process.ErrorDataReceived += (s, e) =>
        {
            var line = e.Data;
            if (string.IsNullOrWhiteSpace(line))
                return;

            logger.LogDebug("[FFmpeg] {Line}", line);

            // Extract time
            var timeMatch = TimeRegex().Match(line);
            if (timeMatch.Success)
            {
                var parsedTime = TimeSpan.Parse(timeMatch.Groups[1].Value);
                var percent = parsedTime.TotalSeconds / task.Info.Duration.TotalSeconds;

                var secondsRemaining = EncodeTimeEstimator.EstimateLive(
                    parsedTime.TotalSeconds,
                    task.Info.Duration.TotalSeconds,
                    stopwatch.Elapsed.TotalSeconds);

                progressCallback?.Invoke((queueItem, percent, currentFPS, currentSpeedX, secondsRemaining));
            }

            // Extract FPS
            var fpsMatch = FPSRegex().Match(line);
            if (fpsMatch.Success)
                currentFPS = (int)double.Parse(fpsMatch.Groups[1].Value);

            // Extract speed
            var speedMatch = SpeedRegex().Match(line);
            if (speedMatch.Success)
                currentSpeedX = double.Parse(speedMatch.Groups[1].Value);
        };


        // Register cancellation callback
        using var registration = queueItem.Cancellation.Token.Register(() =>
        {
            if (!process.HasExited && process.Id != 0)
            {
                try
                {
                    logger.LogInformation("Cancellation requested, stopping process...");
                    process.Kill(entireProcessTree: true);

                }
                catch (Exception ex)
                {
                    // Log the error, and return the error to the user.
                    logger.LogError(ex, "Failed to kill ffprobe process for {FilePath}. {file}", queueItem.Task.OutputPath, queueItem.Task.OutputFileName);
                    task.ErrorMessage = ex.Message;
                }
            }
        });

        process.Start();

        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        try
        {
            await process.WaitForExitAsync(queueItem.Cancellation.Token);

        }
        catch (TaskCanceledException)
        {

        }

        // ---------------------------------------------------------
        // CLEANUP TEMP FILES
        // ---------------------------------------------------------

        if (!String.IsNullOrWhiteSpace(burnSubPath) && File.Exists(burnSubPath)) File.Delete(burnSubPath);
        progressCallback?.Invoke((queueItem, 1, currentFPS, currentSpeedX, 0));

        stopwatch.Stop();
        encodeTimeEstimator.RecordJob(task.PerformanceKey, task.Info.Duration.TotalSeconds, stopwatch.Elapsed.TotalSeconds);

        logger.LogInformation("FFmpeg process completed for {File}", task.File.Path);
    }

    // ---------------------------------------------------------
    // BUILD COMMAND
    // ---------------------------------------------------------
    private static string Build(TranscodeTask task, SmartDecision d, string? burnSubPath)
    {
        var info = task.Info;
        var sb = new StringBuilder();

        sb.Append("-y ");

        //
        // Hardware acceleration (NVENC / AMF / QSV)
        //
        if (d.SelectedHardwareEncoder.Contains("nvenc"))
        {
            sb.Append("-hwaccel cuda ");
            sb.Append("-hwaccel_output_format cuda ");
            sb.Append("-init_hw_device cuda=cuda ");
            sb.Append("-filter_hw_device cuda ");
        }
        else if (d.SelectedHardwareEncoder.Contains("amf"))
        {
            sb.Append("-hwaccel d3d11va ");
            sb.Append("-hwaccel_output_format d3d11 ");
        }
        else if (d.SelectedHardwareEncoder.Contains("qsv"))
        {
            sb.Append("-hwaccel qsv ");
            sb.Append("-hwaccel_output_format qsv ");
        }

        sb.Append($"-i \"{task.File.Path}\" ");

        if (d.IsDirectPlay)
        {
            task.FinalCommand = string.Empty;
            return task.FinalCommand;
        }

        AppendStreamMapping(sb, d);
        AppendVideoEncoding(sb, d);
        AppendFilterGraph(sb, d, burnSubPath, task, info);
        AppendAudioEncoding(sb, d, info);
        AppendSubtitleHandling(sb);
        AppendContainerFlags(sb, d);
        AppendOutputPath(sb, task);

        task.FinalCommand = sb.ToString();
        return task.FinalCommand;
    }

    private static void AppendStreamMapping(StringBuilder sb, SmartDecision d)
    {
        // Always map the primary video track
        sb.Append("-map 0:v:0 ");

        // If AAC fallback is NOT required → map audio once
        if (!d.RequiresAacStereoFallback)
        {
            sb.Append("-map 0:a:0 ");
            return;
        }

        // If AAC fallback IS required → map audio twice (no primary map)
        // Track 0 → AAC stereo fallback (Plex wants this first)
        sb.Append("-map 0:a:0 "); // AAC first
        sb.Append("-map 0:a:0 "); // EAC3 second

        d.Reasons.Add("Adding AAC stereo fallback track for Roku compliance.");
    }


    private static void AppendVideoEncoding(StringBuilder sb, SmartDecision d)
    {
        if (!d.RequiresVideoEncode)
        {
            sb.Append("-c:v copy ");
            return;
        }

        var encoder = d.SelectedHardwareEncoder;
        sb.Append($"-c:v {encoder} ");

        //
        // Pixel format rules
        //
        if (encoder.Contains("nvenc"))
        {
            // NVENC chooses nv12/p010 automatically — DO NOT force pix_fmt
        }
        else if (encoder.Contains("qsv"))
        {
            sb.Append("-pix_fmt nv12 ");
        }
        else if (encoder.Contains("amf"))
        {
            sb.Append("-pix_fmt nv12 ");
        }
        else
        {
            sb.Append(d.TargetBitDepth == 8 ? "-pix_fmt yuv420p " : "-pix_fmt yuv420p10le ");
        }

        if (!string.IsNullOrWhiteSpace(d.TargetProfile))
            sb.Append($"-profile:v {d.TargetProfile} ");

        if (!string.IsNullOrWhiteSpace(d.TargetLevel))
            sb.Append($"-level:v {d.TargetLevel} ");

        if (encoder.Contains("nvenc"))
            sb.Append("-preset p5 ");
    }


    private static void AppendFilterGraph(
    StringBuilder sb,
    SmartDecision d,
    string? burnSubPath,
    TranscodeTask task,
    MediaInfo info)
    {
        var filters = new List<string>();

        bool strict = task.Client.SupportedResolutions?.Length > 0;

        string cpuScale = strict
            ? $"scale={d.TargetWidth}:{d.TargetHeight},setsar=1"
            : $"scale={d.TargetWidth}:{d.TargetHeight}:force_original_aspect_ratio=decrease,setsar=1";

        //
        // Dolby Vision → strip RPU metadata
        //
        if (info.IsDolbyVision)
            filters.Add("dovi_rpu_remove");

        //
        // Hardware scaling
        //
        if (d.SelectedHardwareEncoder.Contains("nvenc"))
        {
            filters.Add(
                $"scale_cuda={d.TargetWidth}:{d.TargetHeight}:force_original_aspect_ratio=decrease,setsar=1"
            );
        }
        else if (d.SelectedHardwareEncoder.Contains("amf"))
        {
            filters.Add(
                $"scale_d3d11va={d.TargetWidth}:{d.TargetHeight}:force_original_aspect_ratio=decrease,setsar=1"
            );
        }
        else if (d.SelectedHardwareEncoder.Contains("qsv"))
        {
            filters.Add($"scale_qsv={d.TargetWidth}:{d.TargetHeight}");
        }
        else
        {
            filters.Add(cpuScale);
        }

        //
        // Subtitle burn‑in (CPU only)
        //
        if (d.SubtitleHandling == SubtitleMode.BurnIn && burnSubPath != null)
        {
            var ffmpegPath = ConvertToFFmpegPath(burnSubPath);
            filters.Add($"subtitles='{ffmpegPath}'");
        }

        if (filters.Count > 0)
            sb.Append($"-vf \"{string.Join(",", filters)}\" ");
    }



    private static void AppendAudioEncoding(StringBuilder sb, SmartDecision d, MediaInfo info)
    {
        // Direct play / direct stream
        if (!d.RequiresAudioEncode && !d.RequiresAacStereoFallback)
        {
            sb.Append("-c:a copy ");
            return;
        }

        // -----------------------------------------------------
        // CASE 1: Single audio track (no AAC fallback required)
        // -----------------------------------------------------
        if (!d.RequiresAacStereoFallback)
        {
            sb.Append($"-c:a {d.SelectedAudioCodec} ");

            if (d.AudioDownmixToStereo)
            {
                sb.Append("-ac 2 ");
                sb.Append("-af \"pan=stereo|FL=0.7*FL+0.3*FC|FR=0.7*FR+0.3*FC\" ");
                d.Reasons.Add("Downmixing multichannel audio to stereo.");
            }
            else
            {
                if (info.AudioChannels > 0)
                    sb.Append($"-ac {d.TargetAudioChannels} ");
            }

            // Bitrate rules
            if (d.SelectedAudioCodec == "aac")
            {
                sb.Append(info.AudioChannels > 2 ? "-b:a 384k " : "-b:a 160k ");
            }
            else if (d.SelectedAudioCodec == "ac3")
            {
                sb.Append("-b:a 640k ");
            }
            else if (d.SelectedAudioCodec == "eac3")
            {
                sb.Append("-b:a 768k ");
            }

            return;
        }

        // -----------------------------------------------------
        // CASE 2: Dual audio track output (Dolby + AAC fallback)
        // -----------------------------------------------------
        // IMPORTANT: No mapping here — mapping is handled in AppendStreamMapping
        // Track 0 → AAC stereo
        sb.Append("-c:a:0 aac ");
        sb.Append("-ac:a:0 2 ");
        sb.Append("-b:a:0 128k ");

        // Track 1 → EAC3 5.1
        sb.Append("-c:a:1 eac3 ");
        sb.Append($"-ac:a:1 {d.TargetAudioChannels} ");
        sb.Append("-b:a:1 768k ");


        d.Reasons.Add("Adding AAC stereo fallback track for Roku compliance.");
    }

    private static void AppendSubtitleHandling(StringBuilder sb)
    {
        // Internal subtitles are ALWAYS stripped.
        // External subtitles handled outside FFmpeg.
        // Burn-in handled via filtergraph.
        sb.Append("-sn ");
    }

    private static void AppendContainerFlags(StringBuilder sb, SmartDecision d)
    {
        if (d.TargetContainer == "mp4")
            sb.Append("-movflags +faststart ");
    }

    private static void AppendOutputPath(StringBuilder sb, TranscodeTask task)
    {
        sb.Append($"\"{task.OutputPath}\"");
    }


    // ---------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------
    private static string ConvertToFFmpegPath(string path)
    {
        // Windows → FFmpeg POSIX path
        // C:\Users\Ryan\file.srt → C:/Users/Ryan/file.srt

        var root = Path.GetPathRoot(path) ?? string.Empty;
        var drive = root.Length > 1 ? root[..1] : string.Empty;
        var withoutDrive = path[root.Length..].TrimStart('\\', '/');

        return $"{drive}\\:/{withoutDrive.Replace("\\", "/")}";
    }



    private static void NormalizeSrtFile(string path)
    {
        var lines = File.ReadAllLines(path)
            .Select(l => l.TrimEnd())
            .ToList();

        var normalized = new List<string>();
        bool lastWasNumber = false;

        foreach (var line in lines)
        {
            if (int.TryParse(line, out _))
            {
                if (!lastWasNumber)
                    normalized.Add("");

                lastWasNumber = true;
            }
            else
            {
                lastWasNumber = false;
            }

            normalized.Add(line);
        }

        File.WriteAllLines(path, normalized);
    }



    [GeneratedRegex(@"time=(\d+:\d+:\d+.\d+)")]
    private static partial Regex TimeRegex();

    [GeneratedRegex(@"fps=\s*(\d+)")]
    private static partial Regex FPSRegex();

    [GeneratedRegex(@"speed=\s*([\d\.]+)x")]
    private static partial Regex SpeedRegex();

}
