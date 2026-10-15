using Dynamensions.EasyVideoConverter.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;

namespace Dynamensions.EasyVideoConverter.Services;

public sealed partial class WinUIPickerService(ILogger<WinUIPickerService> logger) : IPickerService
{

    // ---------------------------------------------------------
    // PICK SINGLE FILE
    // ---------------------------------------------------------
    public async Task<(string FilePath, long Size)?> PickFileAsync()
    {
        LogPickerStart("File");

        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add("*");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        StorageFile? file = await picker.PickSingleFileAsync();

        if (file == null)
        {
            LogPickerCancelled("File");
            return null;
        }

        try
        {
            BasicProperties props = await file.GetBasicPropertiesAsync();
            long size = (long)props.Size;

            LogFilePicked(file.Path, size);
            return (file.Path, size);
        }
        catch (Exception ex)
        {
            LogFilePickFailed(file.Path);
            logger.LogError(ex, "Failed to read file properties for {Path}", file.Path);
            return null;
        }
    }

    // ---------------------------------------------------------
    // PICK FOLDER AND ENUMERATE FILES
    // ---------------------------------------------------------
    public async Task<List<(string FolderPath, long Size)>?> PickFolderFilesAsync()
    {
        LogPickerStart("FolderFiles");

        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        StorageFolder? folder = await picker.PickSingleFolderAsync();

        if (folder == null)
        {
            LogPickerCancelled("FolderFiles");
            return null;
        }

        LogFolderPicked(folder.Path);
        return await EnumerateMediaFilesAsync(folder);
    }

    // ---------------------------------------------------------
    // PICK FOLDER ONLY
    // ---------------------------------------------------------
    public async Task<string?> PickFolderAsync()
    {
        LogPickerStart("Folder");

        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        StorageFolder? folder = await picker.PickSingleFolderAsync();

        if (folder == null)
        {
            LogPickerCancelled("Folder");
            return null;
        }

        LogFolderPicked(folder.Path);
        return folder.Path;
    }

    // ---------------------------------------------------------
    // ENUMERATE MEDIA FILES
    // ---------------------------------------------------------
    private async Task<List<(string FolderPath, long Size)>> EnumerateMediaFilesAsync(StorageFolder folder)
    {
        if (folder == null)
        {
            LogFolderNull();
            return [];
        }

        LogEnumerateStart(folder.Path);

        var results = new List<(string FolderPath, long Size)>();
        var validExtensions = new[] { ".mp4", ".mkv", ".avi", ".mov", ".webm" };

        IReadOnlyList<IStorageItem> items;

        try
        {
            items = await folder.GetItemsAsync();
        }
        catch (Exception ex)
        {
            LogEnumerateFailed(folder.Path);
            logger.LogError(ex, "Failed to enumerate folder {Path}", folder.Path);
            return results;
        }

        foreach (var item in items)
        {
            if (item is StorageFile file)
            {
                var ext = file.FileType.ToLower();

                if (!validExtensions.Contains(ext))
                {
                    LogFileSkipped(file.Path, ext);
                    continue;
                }

                try
                {
                    BasicProperties props = await file.GetBasicPropertiesAsync();
                    long size = (long)props.Size;

                    LogFileAccepted(file.Path, size);
                    results.Add((file.Path, size));
                }
                catch (Exception ex)
                {
                    LogFileReadFailed(file.Path);
                    logger.LogError(ex, "Failed to read file properties for {Path}", file.Path);
                }
            }
            else if (item is StorageFolder subfolder)
            {
                LogSubfolderEnter(subfolder.Path);
                var subResults = await EnumerateMediaFilesAsync(subfolder);
                results.AddRange(subResults);
            }
        }

        LogEnumerateComplete(folder.Path, results.Count);
        return results;
    }

    // ---------------------------------------------------------
    // SOURCE-GENERATED LOGGING
    // ---------------------------------------------------------
    [LoggerMessage(EventId = 1, Level = LogLevel.Information,
        Message = "Picker started → {Type}")]
    private partial void LogPickerStart(string type);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning,
        Message = "Picker cancelled → {Type}")]
    private partial void LogPickerCancelled(string type);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information,
        Message = "File picked → {Path} ({Size} bytes)")]
    private partial void LogFilePicked(string path, long size);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error,
        Message = "Failed to pick file → {Path}")]
    private partial void LogFilePickFailed(string path);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information,
        Message = "Folder picked → {Path}")]
    private partial void LogFolderPicked(string path);

    [LoggerMessage(EventId = 6, Level = LogLevel.Warning,
        Message = "Folder was null during enumeration.")]
    private partial void LogFolderNull();

    [LoggerMessage(EventId = 7, Level = LogLevel.Information,
        Message = "Enumerating media files in → {Path}")]
    private partial void LogEnumerateStart(string path);

    [LoggerMessage(EventId = 8, Level = LogLevel.Error,
        Message = "Failed to enumerate folder → {Path}")]
    private partial void LogEnumerateFailed(string path);

    [LoggerMessage(EventId = 9, Level = LogLevel.Debug,
        Message = "Skipping file → {Path} (Extension={Extension})")]
    private partial void LogFileSkipped(string path, string extension);

    [LoggerMessage(EventId = 10, Level = LogLevel.Information,
        Message = "Accepted media file → {Path} ({Size} bytes)")]
    private partial void LogFileAccepted(string path, long size);

    [LoggerMessage(EventId = 11, Level = LogLevel.Error,
        Message = "Failed to read file properties → {Path}")]
    private partial void LogFileReadFailed(string path);

    [LoggerMessage(EventId = 12, Level = LogLevel.Debug,
        Message = "Entering subfolder → {Path}")]
    private partial void LogSubfolderEnter(string path);

    [LoggerMessage(EventId = 13, Level = LogLevel.Information,
        Message = "Completed enumeration for {Path}. Files found: {Count}")]
    private partial void LogEnumerateComplete(string path, int count);
}
