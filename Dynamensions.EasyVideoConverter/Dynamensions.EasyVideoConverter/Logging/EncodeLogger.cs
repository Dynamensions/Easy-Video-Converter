using Serilog;
using System;
using System.IO;

namespace Dynamensions.EasyVideoConverter.Logging;

public class EncodeLogger : IDisposable
{
    public ILogger Logger { get; }

    public EncodeLogger(string jobId)
    {
        // Set the base folder
        var appFolder = AppContext.BaseDirectory;
        // Set the local folder
        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        // Set the log folder
        var logFolder = Path.Combine(localFolder.Path, "Dynamensions.EasyVideoConverter", "Logs");

        var fileName = Path.Combine(
            logFolder,
            $"{jobId}_{DateTime.Now:yyyyMMdd_HHmmss}.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
            path: "Logs\\Jobs-.log",
            rollingInterval: RollingInterval.Hour,
            retainedFileCountLimit: 2)
            .CreateLogger();


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

    public void Dispose()
    {
        (Logger as IDisposable)?.Dispose();
    }
}