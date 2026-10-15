using Dynamensions.EasyVideoConverter.Core.Helpers;
using Dynamensions.EasyVideoConverter.Core.Models.Profiles;
using Dynamensions.EasyVideoConverter.Core.Serialization;
using Dynamensions.EasyVideoConverter.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Dynamensions.EasyVideoConverter.Services;

public sealed partial class EncoderProfileService : IEncoderProfileService
{
    private readonly string jsonPath;
    private readonly ILogger<EncoderProfileService> logger;

    public EncoderProfileService(ILogger<EncoderProfileService> logger)
    {
        this.logger = logger;

        jsonPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EasyVideoConverter",
            "serverprofiles.json"
        );

        Directory.CreateDirectory(Path.GetDirectoryName(jsonPath)!);
        LogJsonPath(jsonPath);
    }

    // ---------------------------------------------------------
    // PUBLIC: GET ALL SERVER PROFILES
    // ---------------------------------------------------------
    public List<EncoderProfile> GetAll()
    {
        LogLoadingDefaults();

        var defaults = new EncoderProfileCollection().Profiles;
        LogDefaultCount(defaults.Count);

        var custom = LoadCustomProfiles();
        LogCustomCount(custom.Count);

        // Merge custom profiles (override by name)
        foreach (var c in custom)
        {
            var existing = defaults.FirstOrDefault(x => x.Name == c.Name);
            if (existing != null)
            {
                LogDuplicateProfile(c.Name);
                defaults.Remove(existing);
            }

            LogRegisteringProfile(c.Name);
            defaults.Add(c);
        }

        LogFinalProfileCount(defaults.Count);
        return defaults;
    }

    // ---------------------------------------------------------
    // FILTER AVAILABLE PROFILES BASED ON CAPABILITIES
    // ---------------------------------------------------------
    public IEnumerable<EncoderProfile> GetAvailableProfiles(EncoderCapabilities encoderCapabilities)
    {
        LogFilteringCapabilities(
            encoderCapabilities.HasCpu,
            encoderCapabilities.HasNvidia,
            encoderCapabilities.HasIntelQsv,
            encoderCapabilities.HasAmdAmf
        );

        var profiles = GetAll();
        var capable = new List<EncoderProfile>();

        if (profiles.Count > 0)
        {
            if (encoderCapabilities.HasCpu)
                capable.AddRange(profiles.Where(x => x.Name.Contains("CPU")));

            if (encoderCapabilities.HasNvidia)
                capable.AddRange(profiles.Where(x => x.SupportsNvenc));

            if (encoderCapabilities.HasIntelQsv)
                capable.AddRange(profiles.Where(x => x.SupportsQsv));

            if (encoderCapabilities.HasAmdAmf)
                capable.AddRange(profiles.Where(x => x.SupportsAmf));
        }

        LogFilteredCount(capable.Count);
        return capable;
    }

    // ---------------------------------------------------------
    // LOAD CUSTOM JSON PROFILES
    // ---------------------------------------------------------
    private List<EncoderProfile> LoadCustomProfiles()
    {
        LogLoadingJson(jsonPath);

        if (!File.Exists(jsonPath))
        {
            LogJsonMissing(jsonPath);
            return [];
        }

        try
        {
            var json = File.ReadAllText(jsonPath);
            var list = JsonSerializer.Deserialize(
                json,
                EncoderProfileJsonContext.Default.ListEncoderProfile
            ) ?? [];

            LogJsonLoaded(list.Count);
            return list;
        }
        catch (Exception ex)
        {
            LogJsonLoadFailed(jsonPath);
            logger.LogDebug(ex, "JSON parsing error");
            return [];
        }
    }

    // ---------------------------------------------------------
    // SAVE CUSTOM JSON PROFILES
    // ---------------------------------------------------------
    public void SaveCustomProfiles(List<EncoderProfile> profiles)
    {
        LogSavingJson(profiles.Count, jsonPath);

        var json = JsonSerializer.Serialize(
            profiles,
            EncoderProfileJsonContext.Default.ListEncoderProfile
        );

        File.WriteAllText(jsonPath, json);
        LogJsonSaved();
    }

    // ---------------------------------------------------------
    // SOURCE-GENERATED LOGGING
    // ---------------------------------------------------------
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug,
        Message = "Encoder profile JSON path → {Path}")]
    private partial void LogJsonPath(string path);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information,
        Message = "Loading default encoder profiles.")]
    private partial void LogLoadingDefaults();

    [LoggerMessage(EventId = 3, Level = LogLevel.Information,
        Message = "Default encoder profiles count → {Count}")]
    private partial void LogDefaultCount(int count);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information,
        Message = "Custom encoder profiles count → {Count}")]
    private partial void LogCustomCount(int count);

    [LoggerMessage(EventId = 5, Level = LogLevel.Warning,
        Message = "Duplicate encoder profile detected → {Name}. Overwriting.")]
    private partial void LogDuplicateProfile(string name);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information,
        Message = "Registering encoder profile → {Name}")]
    private partial void LogRegisteringProfile(string name);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information,
        Message = "Final encoder profile count → {Count}")]
    private partial void LogFinalProfileCount(int count);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information,
        Message = "Filtering profiles based on capabilities → CPU={Cpu}, NVENC={Nvenc}, QSV={Qsv}, AMF={Amf}")]
    private partial void LogFilteringCapabilities(bool cpu, bool nvenc, bool qsv, bool amf);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information,
        Message = "Filtered encoder profiles count → {Count}")]
    private partial void LogFilteredCount(int count);

    [LoggerMessage(EventId = 10, Level = LogLevel.Information,
        Message = "Loading custom encoder profiles from {Path}")]
    private partial void LogLoadingJson(string path);

    [LoggerMessage(EventId = 11, Level = LogLevel.Warning,
        Message = "Custom encoder profile JSON missing → {Path}")]
    private partial void LogJsonMissing(string path);

    [LoggerMessage(EventId = 12, Level = LogLevel.Information,
        Message = "Loaded {Count} custom encoder profiles.")]
    private partial void LogJsonLoaded(int count);

    [LoggerMessage(EventId = 13, Level = LogLevel.Error,
        Message = "Failed to load custom encoder profiles from {Path}. Using empty list.")]
    private partial void LogJsonLoadFailed(string path);

    [LoggerMessage(EventId = 14, Level = LogLevel.Information,
        Message = "Saving {Count} custom encoder profiles to {Path}")]
    private partial void LogSavingJson(int count, string path);

    [LoggerMessage(EventId = 15, Level = LogLevel.Information,
        Message = "Custom encoder profiles saved successfully.")]
    private partial void LogJsonSaved();
}
