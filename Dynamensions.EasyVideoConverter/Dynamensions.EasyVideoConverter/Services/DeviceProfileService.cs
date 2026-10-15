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

public class DeviceProfileService : IDeviceProfileService
{
    private readonly ILogger logger;
    private readonly string jsonPath;
    private readonly EncoderProfileCollection encoderProfileCollection = new();

    public DeviceProfileService(ILogger<DeviceProfileService> logger)
    {
        this.logger = logger;

        jsonPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EasyVideoConverter",
            "serverprofiles.json"
        );

        Directory.CreateDirectory(Path.GetDirectoryName(jsonPath)!);
    }

    // ---------------------------------------------------------
    // LOAD CUSTOM JSON PROFILES
    // ---------------------------------------------------------
    public List<EncoderProfile> LoadCustomProfiles()
    {
        if (!File.Exists(jsonPath))
            return [];

        try
        {
            var json = File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize(
                json,
                EncoderProfileJsonContext.Default.ListEncoderProfile
                ) ?? [];
        }
        catch
        {
            return [];
        }
    }

    // ---------------------------------------------------------
    // SAVE CUSTOM JSON PROFILES
    // ---------------------------------------------------------
    public void SaveCustomProfiles(List<EncoderProfile> profiles)
    {
        var json = JsonSerializer.Serialize(
            profiles,
            EncoderProfileJsonContext.Default.ListEncoderProfile
        );

        File.WriteAllText(jsonPath, json);
    }


    // ---------------------------------------------------------
    // PUBLIC: GET ALL SERVER PROFILES
    // ---------------------------------------------------------
    public List<EncoderProfile> GetAll()
    {
        using var scope = logger.BeginScope(nameof(GetAll));

        if (logger.IsEnabled(LogLevel.Debug)) logger.LogDebug("Loading default profiles...");
        var defaults = GetDefaultProfiles();
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Loaded {profileCount} profiles.", defaults.Count);
        }


        if (logger.IsEnabled(LogLevel.Debug)) logger.LogDebug("Loading custom profiles...");
        var custom = LoadCustomProfiles();
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Loaded {profileCount} profiles.", custom.Count);
        }


        if (logger.IsEnabled(LogLevel.Debug)) logger.LogDebug("Merging profiles...");
        // Merge custom profiles (override by name)
        foreach (var c in custom)
        {
            var existing = defaults.FirstOrDefault(x => x.Name == c.Name);
            if (existing != null)
                defaults.Remove(existing);

            defaults.Add(c);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Combined {profileCount} profiles.", defaults.Count);
        }

        return defaults;
    }



    // -----------------------------
    // DEFAULT PROFILES
    // -----------------------------
    public List<EncoderProfile> GetDefaultProfiles()
    {
        return encoderProfileCollection.Profiles;
    }

    // -----------------------------
    // GET PROFILE FOR DEVICE
    // -----------------------------
    public EncoderProfile GetProfileForDevice(string deviceName)
    {
        return encoderProfileCollection.GetByName(deviceName);
    }

}
