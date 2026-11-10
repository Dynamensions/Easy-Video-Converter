using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dynamensions.EasyVideoConverter.Core.ViewModels;

public partial class LogsViewModel(ILogService logService, ILogger<LogsViewModel> logger) : ObservableObject
{
    public ObservableCollection<LogEntry> LogEntries { get; } = [];
    public List<LogLine> LogLines { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteLogCommand))]
    private LogEntry? selectedLog;


    public async Task PrepareViewModelAsync()
    {
        var logFiles = await logService.GetLogFilenamesAsync();
        foreach (var logFile in logFiles)
        {
            LogEntries.Add(logFile);
        }
    }

    partial void OnSelectedLogChanged(LogEntry value)
    {
        if (value is not null)
        {
            var lines = value.Content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            LogLines = lines.Select((line, index) => ParseLogLine(line, index++)).ToList();

            OnPropertyChanged(nameof(LogLines));
        }
    }


    [RelayCommand(CanExecute = nameof(CanDeleteLog))]
    private async Task DeleteLogAsync()
    {
        try
        {
            if (SelectedLog is null) return;

            // Delete the file
            logger.LogInformation($"Deleting {SelectedLog.FileName}");
            await logService.DeleteLogAsync(SelectedLog);

            LogEntries.Remove(SelectedLog);
            OnPropertyChanged(nameof(LogEntries));

            SelectedLog = null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting log.");
        }
    }
    private bool CanDeleteLog() => SelectedLog != null;


    [RelayCommand]
    private async Task DeleteAllLogsAsync()
    {
        List<LogEntry> logEntries = new List<LogEntry>(LogEntries);
        SelectedLog = null;

        foreach (var entry in logEntries)
        {
            try
            {
                // Delete the file
                logger.LogInformation($"Deleting {entry.FileName}");
                await logService.DeleteLogAsync(entry);
                LogEntries.Remove(SelectedLog);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting log.");
            }
        }
        OnPropertyChanged(nameof(LogEntries));
    }

    private static LogLine ParseLogLine(string line, int index)
    {
        var match = LogRegex.Match(line);

        if (!match.Success)
        {
            return new LogLine
            {
                RawText = line,
                Message = line
            };
        }

        DateTimeOffset.TryParse(match.Groups["timestamp"].Value, out var timestamp);

        return new LogLine
        {
            Index = index,
            Timestamp = timestamp,
            Level = match.Groups["level"].Value,
            Message = match.Groups["message"].Value,
            RawText = line
        };
    }

    private static readonly Regex LogRegex = new(
        @"^(?<timestamp>\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}\.\d+\s+[+-]\d{2}:\d{2})\s+\[(?<level>[A-Z]+)\]\s+(?<message>.*)$",
        RegexOptions.Compiled);
}
