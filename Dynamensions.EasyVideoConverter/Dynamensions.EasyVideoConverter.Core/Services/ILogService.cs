using Dynamensions.EasyVideoConverter.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dynamensions.EasyVideoConverter.Core.Services
{
    public interface ILogService
    {
        Task<IEnumerable<LogEntry>> GetLogFilenamesAsync();
        Task<string> OpenLogAsync(string filename);
        Task DeleteLogAsync(LogEntry logEntry);
        Task DeleteAllLogsAsync();
        Task DeleteAllJobLogs();
    }
}
