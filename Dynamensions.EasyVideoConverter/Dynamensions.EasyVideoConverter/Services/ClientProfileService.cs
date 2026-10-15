using Dynamensions.EasyVideoConverter.Core.Extensions;
using Dynamensions.EasyVideoConverter.Core.Models.Profiles;
using Dynamensions.EasyVideoConverter.Core.Serialization;
using Dynamensions.EasyVideoConverter.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Dynamensions.EasyVideoConverter.Services;

public partial class ClientProfileService : IClientProfileService
{
    private readonly string jsonPath;
    private readonly ILogger<ClientProfileService> logger;

    public Dictionary<string, ClientProfile> Profiles { get; } = [];

    public ClientProfileService(ILogger<ClientProfileService> logger)
    {
        this.logger = logger;

        jsonPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EasyVideoConverter",
            "clientprofiles.json"
        );

        Directory.CreateDirectory(Path.GetDirectoryName(jsonPath)!);
    }

    // ---------------------------------------------------------
    // LOAD CUSTOM JSON PROFILES
    // ---------------------------------------------------------
    private List<ClientProfile> LoadCustomProfiles()
    {
        LogLoadingJson(jsonPath);

        if (!File.Exists(jsonPath))
        {
            LogJsonLoaded(0);
            return [];
        }

        try
        {
            var json = File.ReadAllText(jsonPath);
            var list = JsonSerializer.Deserialize(
                json,
                EncoderProfileJsonContext.Default.ListClientProfile
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
    public Task SaveCustomProfilesAsync(List<ClientProfile> profiles)
    {
        LogSavingJson(profiles.Count, jsonPath);

        var json = JsonSerializer.Serialize(
            profiles,
            EncoderProfileJsonContext.Default.ListClientProfile
        );

        File.WriteAllText(jsonPath, json);

        LogJsonSaved();
        return Task.CompletedTask;
    }

    // ---------------------------------------------------------
    // XML SCHEMA VALIDATION
    // ---------------------------------------------------------
    private static void Validate(string xmlPath, string xsdPath)
    {
        XmlReaderSettings settings = new()
        {
            ValidationType = ValidationType.Schema,
            Async = true
        };

        settings.Schemas.Add(null, xsdPath);
        settings.ValidationFlags =
            XmlSchemaValidationFlags.ReportValidationWarnings |
            XmlSchemaValidationFlags.ProcessInlineSchema |
            XmlSchemaValidationFlags.ProcessSchemaLocation;

        settings.ValidationEventHandler += (sender, args) =>
        {
            throw new XmlSchemaValidationException(
                $"Validation error in '{xmlPath}': {args.Message}"
            );
        };

        using var reader = XmlReader.Create(xmlPath, settings);
        while (reader.Read()) { }
    }

    // ---------------------------------------------------------
    // LOAD INTERNAL XML PROFILES
    // ---------------------------------------------------------
    public void LoadInternalProfiles()
    {
        using var scope = logger.BeginScope(nameof(LoadInternalProfiles));

        var rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Profiles");
        var xsdPath = Path.Combine(rootPath, "schema", "profile.xsd");

        LogProfileRoot(rootPath);
        LogSchemaPath(xsdPath);

        foreach (var xmlPath in Directory.GetFiles(rootPath, "*.xml", SearchOption.AllDirectories))
        {
            LogXmlDiscovered(xmlPath);

            // Validate XML
            try
            {
                LogXmlValidating(xmlPath);
                Validate(xmlPath, xsdPath);
                LogXmlValidationSucceeded(xmlPath);
            }
            catch (Exception ex)
            {
                LogXmlValidationFailed(xmlPath);
                logger.LogError(ex, "Validation failed for {xmlPath}. Skipping!", xmlPath);
                LogProfileSkipped(xmlPath);
                continue;
            }

            // Parse XML
            LogXmlParsing(xmlPath);

            var doc = XDocument.Load(xmlPath);
            var root = doc.Root;

            if (root == null)
            {
                LogXmlRootMissing(xmlPath);
                LogProfileSkipped(xmlPath);
                continue;
            }

            // Construct profile
            var profile = new ClientProfile
            {
                Name = root.Attribute("name")?.Value ?? throw new Exception("Missing name attribute"),
                DeviceFamily = root.Attribute("family")?.Value ?? throw new Exception("Missing family attribute"),
                DeviceType = Enum.Parse<DeviceType>(root.Attribute("deviceType")?.Value ?? throw new Exception("Missing type attribute")),

                DirectPlayVideoCodecs = root.Element("DirectPlay")?.Elements("VideoCodec").Select(e => e.Value).ToList() ?? [],
                DirectPlayAudioCodecs = root.Element("DirectPlay")?.Elements("AudioCodec").Select(e => e.Value).ToList() ?? [],
                DirectPlayContainers = root.Element("DirectPlay")?.Elements("Container").Select(e => e.Value).ToList() ?? [],

                DirectStreamVideoCodecs = root.Element("DirectStream")?.Elements("VideoCodec").Select(e => e.Value).ToList() ?? [],
                DirectStreamAudioCodecs = root.Element("DirectStream")?.Elements("AudioCodec").Select(e => e.Value).ToList() ?? [],
                DirectStreamContainers = root.Element("DirectStream")?.Elements("Container").Select(e => e.Value).ToList() ?? [],

                PreferredVideoCodec = root.Element("Preferred")?.Element("VideoCodec")?.Value ?? string.Empty,
                PreferredAudioCodec = root.Element("Preferred")?.Element("AudioCodec")?.Value ?? string.Empty,
                PreferredContainer = root.Element("Preferred")?.Element("Container")?.Value ?? string.Empty,

                MaxWidth = (int?)root.Element("Limits")?.Element("MaxWidth") ?? 0,
                MaxHeight = (int?)root.Element("Limits")?.Element("MaxHeight") ?? 0,
                MaxAudioChannels = (int?)root.Element("Limits")?.Element("MaxAudioChannels") ?? 2,

                RequiresStrictH264Compliance = (bool?)root.Element("Requirements")?.Element("StrictH264") ?? false,
                RequiredProfile = root.Element("Requirements")?.Element("Profile")?.Value ?? string.Empty,
                RequiredLevel = root.Element("Requirements")?.Element("Level")?.Value ?? string.Empty,
                Requires8Bit = ((int?)root.Element("Requirements")?.Element("BitDepth") ?? 8) == 8,
                ForceStereoDownmix = (bool?)root.Element("Requirements")?.Element("ForceStereoDownmix") ?? false,
                RequiresAacStereoFallback = (bool?)root.Element("Requirements")?.Element("RequiresAacStereoFallback") ?? false,

                SupportsInternalSubtitles = (bool?)root.Element("Subtitles")?.Element("Internal") ?? false,
                SupportsExternalSubtitles = (bool?)root.Element("Subtitles")?.Element("External") ?? true,
                SupportsSrtSubtitles = (bool?)root.Element("Subtitles")?.Element("SRT") ?? true,
                SupportsPgsSubtitles = (bool?)root.Element("Subtitles")?.Element("PGS") ?? false,
                SupportsMovTextSubtitles = (bool?)root.Element("Subtitles")?.Element("MovText") ?? false,

                Description = root.Element("Description")?.Value
            };

            LogProfileConstructed(profile.Name);

            if (Profiles.ContainsKey(profile.Name))
                LogDuplicateProfile(profile.Name);

            Profiles[profile.Name] = profile;
            LogProfileRegistered(profile.Name);

            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("Profile:{dump}", profile.DumpPropertiesFormatted());
        }

        LogInternalProfileSummary(Profiles.Count);
    }

    // ---------------------------------------------------------
    // SOURCE-GENERATED LOGGING
    // ---------------------------------------------------------
    [LoggerMessage(EventId = 1, Level = LogLevel.Information,
        Message = "Loading custom client profiles from {Path}")]
    private partial void LogLoadingJson(string path);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning,
        Message = "Failed to load custom profiles from {Path}. Using empty list.")]
    private partial void LogJsonLoadFailed(string path);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information,
        Message = "Loaded {Count} custom client profiles.")]
    private partial void LogJsonLoaded(int count);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information,
        Message = "Saving {Count} custom client profiles to {Path}")]
    private partial void LogSavingJson(int count, string path);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information,
        Message = "Custom client profiles saved successfully.")]
    private partial void LogJsonSaved();

    [LoggerMessage(EventId = 6, Level = LogLevel.Debug,
        Message = "Discovered XML profile → {Path}")]
    private partial void LogXmlDiscovered(string path);

    [LoggerMessage(EventId = 7, Level = LogLevel.Debug,
        Message = "Validating XML profile → {Path}")]
    private partial void LogXmlValidating(string path);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information,
        Message = "XML validation succeeded → {Path}")]
    private partial void LogXmlValidationSucceeded(string path);

    [LoggerMessage(EventId = 9, Level = LogLevel.Error,
        Message = "XML validation failed → {Path}")]
    private partial void LogXmlValidationFailed(string path);

    [LoggerMessage(EventId = 10, Level = LogLevel.Trace,
        Message = "Parsing XML profile → {Path}")]
    private partial void LogXmlParsing(string path);

    [LoggerMessage(EventId = 11, Level = LogLevel.Warning,
        Message = "XML root element missing → {Path}. Skipping.")]
    private partial void LogXmlRootMissing(string path);

    [LoggerMessage(EventId = 12, Level = LogLevel.Trace,
        Message = "Constructed client profile → {Name}")]
    private partial void LogProfileConstructed(string name);

    [LoggerMessage(EventId = 13, Level = LogLevel.Warning,
        Message = "Duplicate profile name detected → {Name}. Overwriting.")]
    private partial void LogDuplicateProfile(string name);

    [LoggerMessage(EventId = 14, Level = LogLevel.Information,
        Message = "Registered client profile → {Name}")]
    private partial void LogProfileRegistered(string name);

    [LoggerMessage(EventId = 15, Level = LogLevel.Information,
        Message = "Loaded {Count} internal client profiles.")]
    private partial void LogInternalProfileSummary(int count);

    [LoggerMessage(EventId = 16, Level = LogLevel.Debug,
        Message = "Using internal profile root → {Path}")]
    private partial void LogProfileRoot(string path);

    [LoggerMessage(EventId = 17, Level = LogLevel.Trace,
        Message = "Using XML schema → {Path}")]
    private partial void LogSchemaPath(string path);

    [LoggerMessage(EventId = 18, Level = LogLevel.Warning,
        Message = "Skipping profile → {Path}")]
    private partial void LogProfileSkipped(string path);
}
