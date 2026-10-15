using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dynamensions.EasyVideoConverter.Core.Helpers;
using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Models.Enums;
using Dynamensions.EasyVideoConverter.Core.Models.Profiles;
using Dynamensions.EasyVideoConverter.Core.Models.Subtitles;
using Dynamensions.EasyVideoConverter.Core.Services;
using System.Collections.ObjectModel;

namespace Dynamensions.EasyVideoConverter.Core.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService settingsService;
    private readonly IPickerService pickerService;
    private readonly Dictionary<string, string> filenamePresets = Models.FilenamePatternPresets.Presets;

    public ObservableCollection<SubtitleLanguageItem> Languages { get; } = [];
    public ObservableCollection<BurnInLanguageItem> BurninLanguages { get; } = [];
    public ObservableCollection<string> FilenamePatternPresets { get; } = [];

    [ObservableProperty]
    private bool isSaving;

    public SettingsViewModel(ISettingsService settingsService, IPickerService pickerService)
    {
        this.settingsService = settingsService;
        this.pickerService = pickerService;

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

        FilenamePatternPresets = new ObservableCollection<string>(filenamePresets.Keys);
        SelectedFilenamePatternPreset = settingsService.Settings.SelectedPatternPreset;

        OutputDirectory = settingsService.Settings.OutputFolder;

        FfmpegPath = settingsService.Settings.FFmpegPath;
        FfprobePath = settingsService.Settings.FFprobePath;
    }

    // -----------------------------
    // SETTINGS
    // -----------------------------
    [ObservableProperty]
    private string? ffmpegPath;

    [ObservableProperty]
    private string? ffprobePath;

    [ObservableProperty]
    private EncoderHardware hardware = new()
    {
        SupportsNvenc = true,
        SupportsHevc = true,
        SupportsAv1 = true,
        CpuTier = 4,
        GpuTier = 4,
        MemoryGB = 32
    };

    [ObservableProperty]
    private BurnInLanguageItem selectedBurnInLanguage;

    [ObservableProperty]
    private string? outputDirectory;

    [ObservableProperty]
    private string? selectedFilenamePatternPreset;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilenamePreview))]
    private string? filenamePattern;



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


        settingsService.Settings.PreferredBurninLanguage = SelectedBurnInLanguage.Code;

        settingsService.Settings.PreferredSubtitleLanguages =
           [.. Languages.Where(l => l.IsSelected).Select(l => l.Code)];

        if (SelectedFilenamePatternPreset != null)
            settingsService.Settings.SelectedPatternPreset = SelectedFilenamePatternPreset;

        if (FfmpegPath != null) settingsService.Settings.FFmpegPath = FfmpegPath;
        if (FfprobePath != null) settingsService.Settings.FFprobePath = FfprobePath;

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
        var path = await pickerService.PickFileAsync();
        if (path != null)
            FfprobePath = path.Value.FilePath;
    }
}