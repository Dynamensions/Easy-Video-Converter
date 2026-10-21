using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Services;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamensions.EasyVideoConverter.Services;

public class BootstrapService : IBootstrapService
{

    // BtbN releases claim this build is x86 and x64.
    private const string FfmpegUrlx86 = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";
    // Using the fill build from Gyan
    private const string FfmpegUrlx64 = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-full.7z";
    // BtbN release for the arm64. Gyan full is only x64.
    private const string FfmpegUrlarm64 = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-winarm64-gpl.zip";

    // where the binaries are stored.
    private readonly string _ffmpegDir;
    private bool _initialized;
    private readonly ILogger _logger;

    public bool IsInitialized => _initialized;


    /// <summary>
    /// Initializes a new <see cref="BootstrapService"/>
    /// </summary>
    /// <param name="settingsService">The <see cref="ISettingsService"/></param>
    public BootstrapService(ISettingsService settingsService, ILogger<BootstrapService> logger)
    {
        // set the ffmpeg directory. This is stored as a read-only string in the settings service.
        _ffmpegDir = settingsService.FfMpegDirectory;
        // create the directory if it doesn't exist.
        Directory.CreateDirectory(settingsService.FfMpegDirectory);
        _logger = logger;
    }

    public bool Verify()
    {
        // check if required files exist (ffmpeg, ffmpeg)
        var ffmpegExists = Path.Exists(Path.Combine(_ffmpegDir, "ffmpeg.exe"));
        var ffprobeExists = Path.Exists(Path.Combine(_ffmpegDir, "ffprobe.exe"));

        // if both exists, we're good.
        return ffmpegExists && ffprobeExists;
    }

    public async Task PrepareDependenciesAsync(IProgress<PrepareProgress> progress, CancellationToken cancellationToken)
    {
        try
        {
            // a gate in case this method is called more than once.
            if (_initialized) return;

            // probably redundant, but the call is not expensive so keep it.
            Verify();

            // one is missing. re-download...
            var archivePath = Path.Combine(_ffmpegDir, "ffmpeg.7z");

            // configure the url according to OS architecture
            var ffmpegUrl = GetFfmpegDownloadUrl();

            // download the dependencies
            await DownloadWithProgressAsync("Downloading FFMPEG", ffmpegUrl, archivePath, progress, cancellationToken);
            // add more as needed.

            // extract the dependencies 
            await ExtractWithProgressAsync(archivePath, progress, cancellationToken);
            // add more as needed.

            // move the files needed by the app to the root.
            MoveFiles(progress, cancellationToken);
            // clean up the root
            Cleanup(archivePath, progress, cancellationToken);
            // report the progress is complete.
            progress.Report(new PrepareProgress("Complete", false));

            ValidateCoreDependancies();

            _initialized = true;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "There was an error preparing the application.");
        }
    }

    /// <summary>
    /// Gets the url according to the <see cref="RuntimeInformation.ProcessArchitecture"/>
    /// </summary>
    /// <returns>A <see cref="string"/> containing the FFMPEG url.</returns>
    /// <exception cref="PlatformNotSupportedException">The application is not in one of the acceptable architectures</exception>
    public static string GetFfmpegDownloadUrl()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => FfmpegUrlx64,
            Architecture.X86 => FfmpegUrlx86,
            Architecture.Arm64 => FfmpegUrlarm64,
            _ => throw new PlatformNotSupportedException("Unsupported architecture")
        };
    }

    /// <summary>
    /// Downloads the archive using a progress.
    /// </summary>
    /// <param name="status">The current status to show.</param>
    /// <param name="url">The url of the FFMPEG archive</param>
    /// <param name="destinationPath">The destination directory on where to place the archive.</param>
    /// <param name="progress">A <see cref="IProgress{PrepareProgress}"/> callback for the download method.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> token used to stop operations.</param>
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

    /// <summary>
    /// Extracts the archive with progress.
    /// </summary>
    /// <param name="archivePath">The path to the archive.</param>
    /// <param name="progress">A <see cref="IProgress{PrepareProgress}"/> callback for the download method.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> token used to stop operations.</param>
    private async Task ExtractWithProgressAsync(string archivePath, IProgress<PrepareProgress> progress, CancellationToken cancellationToken)
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
                },
                cancellationToken);
            }
        }
    }

    /// <summary>
    /// Moves the extracted dependencies to the root ffmpeg folder with progress.
    /// </summary>
    /// <param name="progress">A <see cref="IProgress{PrepareProgress}"/> callback for the download method.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> token used to stop operations.</param>
    private void MoveFiles(IProgress<PrepareProgress> progress, CancellationToken cancellationToken)
    {
        PrepareProgress prepareProgress = new("Preparing dependencies...", false);
        // Move DLLs from nested folders to ffmpegDir
        progress.Report(prepareProgress);

        foreach (var file in Directory.GetFiles(_ffmpegDir, "*.exe", SearchOption.AllDirectories))
        {
            var dest = Path.Combine(_ffmpegDir, Path.GetFileName(file));
            if (!File.Exists(dest)) File.Move(file, dest);

            if (cancellationToken.IsCancellationRequested) return;
        }

        foreach (var file in Directory.GetFiles(_ffmpegDir, "*.dll", SearchOption.AllDirectories))
        {
            var dest = Path.Combine(_ffmpegDir, Path.GetFileName(file));
            if (!File.Exists(dest)) File.Move(file, dest);

            if (cancellationToken.IsCancellationRequested) return;
        }
    }

    /// <summary>
    /// Deletes the archive and extracted dependencies with progress.
    /// </summary>
    /// <param name="archivePath"></param>
    /// <param name="progress">A <see cref="IProgress{PrepareProgress}"/> callback for the download method.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> token used to stop operations.</param>
    private void Cleanup(string archivePath, IProgress<PrepareProgress> progress, CancellationToken cancellationToken)
    {
        PrepareProgress prepareProgress = new("Cleaning up...", false);

        progress.Report(prepareProgress);

        // Delete archive
        File.Delete(archivePath);

        // Delete subdirectories.
        foreach (var d in Directory.EnumerateDirectories(_ffmpegDir))
        {
            Directory.Delete(d, true);
            if (cancellationToken.IsCancellationRequested) return;
        }
    }

    /// <summary>
    /// A helper method to check if the required dependencies are present.
    /// </summary>
    private void ValidateCoreDependancies()
    {
        Require("ffmpeg.exe");
        Require("ffprobe.exe");
    }

    /// <summary>
    /// A helper method to determine if the given file is present.
    /// </summary>
    /// <param name="file">The name of the file.</param>
    /// <exception cref="FileNotFoundException">Thrown when the file is not found.</exception>
    private void Require(string file)
    {
        var path = Path.Combine(_ffmpegDir, file);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Missing FFmpeg library: {file}");
    }

    /// <summary>
    /// Formats the file size into a human-readable format.
    /// </summary>
    /// <param name="bytes">The bytes to format.</param>
    /// <returns>A human-readable string for the bytes.</returns>
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

    public Task ExitAppAsync()
    {
        App.Current.Exit();
        return Task.CompletedTask;
    }

}