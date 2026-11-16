using Serilog;
using System;
using System.IO;
using System.Linq;

namespace Dynamensions.EasyVideoConverter.Logging;

public partial class EncodeLogger : IDisposable
{
    // Set the base folder
    private readonly string _appFolder = AppContext.BaseDirectory;
    // Set the local folder
    private readonly Windows.Storage.StorageFolder _localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
    // Set the log folder

    public ILogger Logger { get; }

    public EncodeLogger(string jobId)
    {
        var fileName = Path.Combine(
            App.LogPath,
            $"job-{jobId}_{DateTime.Now:yyyyMMdd_HHmmss}.log");

        // Create directory
        Directory.CreateDirectory(App.LogPath);

#if DEBUG
        Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(fileName)
            .CreateLogger();
#else
        Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(fileName)
            .CreateLogger();
#endif
    }

    public static void CleanLogs()
    {
        var logFiles = Directory.GetFiles(App.LogPath, "job-*.log");
        var filenames = logFiles.Select(Path.GetFileName);
        foreach (var log in filenames)
        {
            try
            {
                if (log != null) File.Delete(log);
            }
            catch
            {
            }
        }
    }

    public void Dispose()
    {
        (Logger as IDisposable)?.Dispose();
        GC.SuppressFinalize(this);
    }
}