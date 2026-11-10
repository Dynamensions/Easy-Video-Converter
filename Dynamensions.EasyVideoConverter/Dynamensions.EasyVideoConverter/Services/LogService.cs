using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Services;
using Dynamensions.EasyVideoConverter.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dynamensions.EasyVideoConverter.Services
{
    public class LogService : ILogService
    {
        // Set the log folder
        private readonly string logFolder = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Dynamensions.EasyVideoConverter", "Logs");

        public Task DeleteAllJobLogs()
        {
            using var encodeLogger = new EncodeLogger("cleanup");
            encodeLogger.CleanLogs();
            return Task.CompletedTask;
        }

        public Task DeleteAllLogsAsync()
        {
            var logFiles = Directory.GetFiles(logFolder);
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

            return Task.CompletedTask;
        }

        public Task DeleteLogAsync(LogEntry logEntry)
        {
            var logFilePath = Path.Combine(logFolder, logEntry.FileName);
            File.Delete(logFilePath);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<LogEntry>> GetLogFilenamesAsync()
        {
            var logFiles = Directory.GetFiles(logFolder);
            var filenames = logFiles.Select(Path.GetFileName);
            List<LogEntry> logFilesList = [];

            foreach (var filename in filenames)
            {
                if (!string.IsNullOrWhiteSpace(filename))
                {
                    var timestamp = File.GetLastWriteTime(Path.Combine(logFolder, filename));
                    // Read the content of the log file using a shared read lock to avoid file access issues
                    using var stream = new FileStream(Path.Combine(logFolder, filename), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream);
                    var content = await reader.ReadToEndAsync();
                    logFilesList.Add(new LogEntry { FilePath = filename, Timestamp = timestamp, Content = content });
                }
            }

            return logFilesList.AsEnumerable();
        }

        public Task<string> OpenLogAsync(string filename)
        {
            var logFilePath = Path.Combine(logFolder, filename);
            return Task.FromResult(logFilePath);
        }
    }
}
