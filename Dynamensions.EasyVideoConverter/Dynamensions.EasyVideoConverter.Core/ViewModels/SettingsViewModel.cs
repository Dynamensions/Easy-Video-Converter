using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dynamensions.EasyVideoConverter.Core.Helpers;
using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Models.Enums;
using Dynamensions.EasyVideoConverter.Core.Models.Profiles;
using Dynamensions.EasyVideoConverter.Core.Models.Subtitles;
using Dynamensions.EasyVideoConverter.Core.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Dynamensions.EasyVideoConverter.Core.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ILogger logger;
    private readonly ISettingsService settingsService;
    private readonly IPickerService pickerService;
    private readonly IBootstrapService bootstrapService;
    private readonly Dictionary<string, string> filenamePresets = Models.FilenamePatternPresets.Presets;

    public ObservableCollection<SubtitleLanguageItem> Languages { get; } = [];
    public ObservableCollection<BurnInLanguageItem> BurninLanguages { get; } = [];
    public ObservableCollection<string> FilenamePatternPresets { get; } = [];

    [ObservableProperty]
    public partial bool IsSaving { get; set; }

    // -----------------------------
    // SETTINGS
    // -----------------------------
    [ObservableProperty]
    public partial string? FfmpegPath { get; set; }

    [ObservableProperty]
    public partial string? FfProbePath { get; set; }

    [ObservableProperty]
    public partial bool? IsUpScalingVideo { get; set; }

    [ObservableProperty]
    public partial EncoderHardware Hardware { get; set; } = new()
    {
        SupportsNvenc = true,
        SupportsHevc = true,
        SupportsAv1 = true,
        CpuTier = 4,
        GpuTier = 4,
        MemoryGB = 32
    };

    [ObservableProperty]
    public partial BurnInLanguageItem SelectedBurnInLanguage { get; set; }

    [ObservableProperty]
    public partial string? OutputDirectory { get; set; }

    [ObservableProperty]
    public partial string? SelectedFilenamePatternPreset { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilenamePreview))]
    public partial string? FilenamePattern { get; set; }


    public SettingsViewModel(ILogger<SettingsViewModel> logger, ISettingsService settingsService, IPickerService pickerService, IBootstrapService bootstrapService)
    {
        this.logger = logger;
        this.settingsService = settingsService;
        this.pickerService = pickerService;
        this.bootstrapService = bootstrapService;

        // Populate list with common languages
        var all = SubtitleLanguageCatalog.All;

        foreach (var (Code, Name) in all)
        {
            Languages.Add(new SubtitleLanguageItem
            {
                Code = Code,
                Name = Name,
                IsSelected = settingsService.Settings.PreferredSubtitleLanguages.Contains(Code)
            });
        }

        BurninLanguages.Add(new BurnInLanguageItem { Name = "None", Code = null });
        foreach (var (Code, Name) in all)
        {
            BurninLanguages.Add(new BurnInLanguageItem
            {
                Code = Code,
                Name = Name
            });
        }

        SelectedBurnInLanguage = BurninLanguages.Single(x => x.Code == settingsService.Settings.PreferredBurninLanguage);

        FilenamePatternPresets = [with(filenamePresets.Keys)];
        SelectedFilenamePatternPreset = settingsService.Settings.SelectedPatternPreset;

        OutputDirectory = settingsService.Settings.OutputFolder;
    }

    partial void OnSelectedFilenamePatternPresetChanged(string? value)
    {
        if (value == null) FilenamePattern = null;
        else if (value != "Custom") FilenamePattern = filenamePresets[value];
    }

    public string? FilenamePreview
    {
        get
        {
            if (FilenamePattern == null) return null;

            // Use dummy objects for preview
            var videoSummary = new VideoSummary
            {
                Codec = "hevc",
                Profile = "Main10",
                Level = "L5.1",
                BitDepth = "10bit",
                Width = 1920,
                Height = 1080,
                IsHdr10 = true,
                Color = "SD",
                IsDolbyVision = true,
                IsHdr10Plus = true,
            };

            var audioSummary = new AudioSummary
            {
                Codec = "aac",
                Channels = 6,
                AudioBitrateKbps = 44,
                IsAtmos = true,
                RequiresAacStereoFallback = true
            };



            var client = new ClientProfile { Name = "Roku", RequiresAacStereoFallback = false };
            var encoder = new EncoderProfile { Name = "NVENC" };
            var media = new MediaInfo { PrimaryAudio = new AudioTrack() { Codec = "aac" }, FrameRate = 23.976 };
            var subtitleMode = SubtitleMode.Extract;
            var burnInSubtitle = new SubtitleTrack { Language = "en" };
            var externalSubtitles = new List<SubtitleTrack>
            {
                new() { Language = "en" },
                new() { Language = "fr" },
            };

            return FilenamePatternEngine.Apply(
                 FilenamePattern,
                 "ExampleVideo.mkv",
                 videoSummary,
                 audioSummary,
                 client,
                 encoder,
                 subtitleMode,
                 burnInSubtitle,
                 externalSubtitles,
                 media);
        }
    }


    // -----------------------------
    // COMMAND: SAVE SETTINGS
    // -----------------------------
    [RelayCommand]
    public void Save()
    {
        IsSaving = true;

        settingsService.Settings.UpscaleVideo = IsUpScalingVideo ?? false;

        settingsService.Settings.PreferredBurninLanguage = SelectedBurnInLanguage.Code;

        settingsService.Settings.PreferredSubtitleLanguages =
           [.. Languages.Where(l => l.IsSelected).Select(l => l.Code)];

        if (SelectedFilenamePatternPreset != null)
            settingsService.Settings.SelectedPatternPreset = SelectedFilenamePatternPreset;


        settingsService.Save();

        IsSaving = false;
    }

    [RelayCommand]
    private async Task BrowseOutputDirectoryAsync()
    {
        var folder = await pickerService.PickFolderAsync();
        if (folder != null)
            OutputDirectory = folder;
    }


    [RelayCommand]
    private async Task BrowseFFMpegPathAsync()
    {
        var path = await pickerService.PickFileAsync();
        if (path != null)
            FfmpegPath = path.Value.FilePath;
    }

    [RelayCommand]
    private async Task BrowseFFProbePathAsync()
    {
        logger.LogInformation("Browsing for FFProbe path.");
        var path = await pickerService.PickFileAsync();
        if (path == null) return;

        // copy the files to the local ffmpeg directory. Overwrite if they already exist.
        logger.LogInformation("Copying FFMPEG binaries from selected path.");
        var (FFMPegCopied, FFProbeCopied) = await bootstrapService.CopyFFMpegBinariesFromPath(path.Value.FilePath);

        if (!FFMPegCopied && !FFProbeCopied)
        {
            logger.LogError("Failed to copy FFMPEG binaries.");
        }

        if (FFMPegCopied) logger.LogInformation("FFMPEG copied successfully.");
        if (FFProbeCopied) logger.LogInformation("FFProbe copied successfully.");
    }
}