using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Services;
using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamensions.EasyVideoConverter.Services;

public class FfmpegBootstrapService : IFfmpegBootstrapService
{
    private const string FfmpegUrl =
        "https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-full.7z";

    private readonly string _ffmpegDir;
    private bool _initialized;

    public bool IsInitialized => _initialized;

    public FfmpegBootstrapService(ISettingsService settingsService)
    {
        _ffmpegDir = settingsService.FfMpegDirectory;
        Directory.CreateDirectory(settingsService.FfMpegDirectory);
    }

    public bool Verify()
    {
        // check if required fiels exists (ffmpeg, ffmpeg)
        var ffmpegExists = Path.Exists(Path.Combine(_ffmpegDir, "ffmpeg.exe"));
        var ffprobeExists = Path.Exists(Path.Combine(_ffmpegDir, "ffprobe.exe"));

        // if both exists, we're good.
        return ffmpegExists && ffprobeExists;
    }


    public async Task InitializeAsync(IProgress<PrepareProgress> progress, CancellationToken cancellationToken)
    {
        if (_initialized)
            return;

        Verify();

        // one is missing. redownload...
        var archivePath = Path.Combine(_ffmpegDir, "ffmpeg.7z");

        await DownloadWithProgressAsync("Downloading FFMPEG", FfmpegUrl, archivePath, progress, cancellationToken);
        await ExtractWithProgressAsync(archivePath, progress);
        MoveFiles(progress);
        Cleanup(archivePath, progress);

        progress.Report(new PrepareProgress("Complete", false));

        ValidateCoreDependancies();

        _initialized = true;
    }

    public string GetFfmpegPath() => _ffmpegDir;

    private static async Task DownloadWithProgressAsync(string status, string url, string destinationPath, IProgress<PrepareProgress> progress, CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        PrepareProgress prepareProgress = new(status);

        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength;

        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var output = File.Create(destinationPath);

        var buffer = new byte[81920]; // 80 KB chunks
        long totalRead = 0;
        int read;

        while ((read = await input.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            totalRead += read;

            if (contentLength.HasValue)
            {
                double percent = (double)totalRead / contentLength.Value;
                string percentText = percent.ToString("P1").PadLeft(6); // " 12.3%" → always 6 chars
                string downloadedText = FormatBytes(totalRead);
                string totalText = FormatBytes(contentLength.Value);

                prepareProgress.Current = totalRead;
                prepareProgress.Total = contentLength.Value;
                prepareProgress.PercentMessage = $"{downloadedText} / {totalText} ({percentText})";

                progress.Report(prepareProgress);
            }
        }
    }

    private async Task ExtractWithProgressAsync(string archivePath, IProgress<PrepareProgress> progress)
    {
        PrepareProgress prepareProgress = new("Extracting files...");
        using var archive = ArchiveFactory.OpenArchive(archivePath);

        var total = archive.Entries.Count();
        var current = 0;

        foreach (var entry in archive.Entries)
        {
            double percent = (double)current++ / (double)total;

            prepareProgress.StatusMessage = $"Extracting {entry.Key} [{current} or {total}]";
            prepareProgress.Current = current;
            prepareProgress.Total = total;
            prepareProgress.PercentMessage = $"{current} / {total}";

            progress.Report(prepareProgress);

            if (!entry.IsDirectory && entry.Key != null)
            {
                var outPath = Path.Combine(_ffmpegDir, entry.Key);
                Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);

                await entry.WriteToFileAsync(outPath, new ExtractionOptions()
                {
                    ExtractFullPath = true,
                    Overwrite = true
                });
            }
        }
    }

    private void MoveFiles(IProgress<PrepareProgress> progress)
    {
        PrepareProgress prepareProgress = new("Preparing dependancies...", false);
        // Move DLLs from nested folders to ffmpegDir
        progress.Report(prepareProgress);

        foreach (var file in Directory.GetFiles(_ffmpegDir, "*.exe", SearchOption.AllDirectories))
        {
            var dest = Path.Combine(_ffmpegDir, Path.GetFileName(file));
            if (!File.Exists(dest))
                File.Move(file, dest);
        }

        foreach (var file in Directory.GetFiles(_ffmpegDir, "*.dll", SearchOption.AllDirectories))
        {
            var dest = Path.Combine(_ffmpegDir, Path.GetFileName(file));
            if (!File.Exists(dest))
                File.Move(file, dest);
        }
    }

    private void Cleanup(string archivePath, IProgress<PrepareProgress> progress)
    {
        PrepareProgress prepareProgress = new("Preparing dependancies...", false);

        progress.Report(prepareProgress);

        // Delete archive
        File.Delete(archivePath);

        // Delete subdirectories.
        foreach (var d in Directory.EnumerateDirectories(_ffmpegDir))
        {
            Directory.Delete(d, true);
        }
    }

    private void ValidateCoreDependancies()
    {
        Require("ffmpeg.exe");
        Require("ffprobe.exe");
    }

    private void Require(string file)
    {
        var path = Path.Combine(_ffmpegDir, file);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Missing FFmpeg library: {file}");
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.0} {sizes[order]}";
    }


}