using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Serialization;
using Dynamensions.EasyVideoConverter.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using Windows.Storage;

namespace Dynamensions.EasyVideoConverter.Services;

public sealed partial class SettingsService : ISettingsService
{
    public event Action? SettingsChanged;


    private readonly string settingsPath;
    private readonly ILogger<SettingsService> logger;

    public AppSettings Settings { get; private set; } = default!;
    public string FfMpegDirectory { get; }

    public SettingsService(ILogger<SettingsService> logger)
    {
        this.logger = logger;
        var baseLocal = ApplicationData.Current.LocalFolder.Path;
        // Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)

        var folder = Path.Combine(baseLocal,
            "Dynamensions.EasyVideoConverter");

        FfMpegDirectory = Path.Combine(baseLocal,
            "Dynamensions",
            "EasyVideoConverter",
            "ffmpeg"
        );

        Directory.CreateDirectory(folder);

        settingsPath = Path.Combine(folder, "settings.json");
        LogSettingsPath(settingsPath);

        Load();
    }

    // ---------------------------------------------------------
    // LOAD SETTINGS
    // ---------------------------------------------------------
    private void Load()
    {
        LogLoadStart(settingsPath);

        if (!File.Exists(settingsPath))
        {
            LogSettingsMissing(settingsPath);
            Settings = new AppSettings();
            Save();
            return;
        }

        try
        {
            var json = File.ReadAllText(settingsPath);
            Settings = JsonSerializer.Deserialize(
                json,
                EncoderProfileJsonContext.Default.AppSettings
            ) ?? new AppSettings();

            LogLoadSuccess();
        }
        catch (Exception ex)
        {
            LogLoadFailed(settingsPath);
            logger.LogError(ex, "Failed to load settings from {Path}", settingsPath);

            Settings = new AppSettings();
            Save();
        }
    }

    // ---------------------------------------------------------
    // SAVE SETTINGS
    // ---------------------------------------------------------
    public void Save()
    {
        LogSaveStart(settingsPath);

        try
        {
            var json = JsonSerializer.Serialize(
                Settings,
                EncoderProfileJsonContext.Default.AppSettings
            );

            File.WriteAllText(settingsPath, json);

            LogSaveSuccess();
            SettingsChanged?.Invoke();
        }
        catch (Exception ex)
        {
            LogSaveFailed(settingsPath);
            logger.LogError(ex, "Failed to save settings to {Path}", settingsPath);
        }
    }

    // ---------------------------------------------------------
    // SOURCE-GENERATED LOGGING
    // ---------------------------------------------------------
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug,
        Message = "Settings file path → {Path}")]
    private partial void LogSettingsPath(string path);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information,
        Message = "Loading settings from {Path}")]
    private partial void LogLoadStart(string path);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning,
        Message = "Settings file missing → {Path}. Creating defaults.")]
    private partial void LogSettingsMissing(string path);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information,
        Message = "Settings loaded successfully.")]
    private partial void LogLoadSuccess();

    [LoggerMessage(EventId = 5, Level = LogLevel.Error,
        Message = "Failed to load settings from {Path}.")]
    private partial void LogLoadFailed(string path);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information,
        Message = "Saving settings to {Path}")]
    private partial void LogSaveStart(string path);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information,
        Message = "Settings saved successfully.")]
    private partial void LogSaveSuccess();

    [LoggerMessage(EventId = 8, Level = LogLevel.Error,
        Message = "Failed to save settings to {Path}.")]
    private partial void LogSaveFailed(string path);
}
