//using CommunityToolkit.Mvvm.ComponentModel;
//using CommunityToolkit.Mvvm.Input;
//using Dynamensions.EasyVideoConverter.Models;
//using Dynamensions.EasyVideoConverter.Services;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Threading.Tasks;

namespace Dynamensions.EasyVideoConverter.Core.ViewModels;

public partial class MainViewModel// : ObservableObject
{

    //    [ObservableProperty]
    //    public partial ObservableCollection<ServerProfile> ServerProfiles { get; set; }

    //    [ObservableProperty]
    //    public partial List<string> DecisionExplanation { get; set; } = new();

    //    [ObservableProperty]
    //    public partial int ConfidenceScore { get; set; }

    //    [ObservableProperty]
    //    public partial List<string> ConfidenceExplanation { get; set; } = new();


    //    [ObservableProperty]
    //    public partial ServerProfile? SelectedServerProfile { get; set; }

    //    [ObservableProperty]
    //    public partial MediaFile? SelectedFile { get; set; }

    //    [ObservableProperty]
    //    public partial MediaInfo? MediaInfo { get; set; }

    //    [ObservableProperty]
    //    public partial SmartDecision? Decision { get; set; }

    //    [ObservableProperty]
    //    public partial TranscodeTask? CurrentTask { get; set; }

    //    [ObservableProperty]
    //    public partial string FinalCommand { get; set; }

    //    [ObservableProperty]
    //    public partial bool IsBusy { get; set; }

    //    // ---------------------------------------------------------
    //    // SERVICES
    //    // ---------------------------------------------------------
    //    private readonly ServerProfileService _serverProfileService;
    //    private readonly FfprobeService _ffprobeService;
    //    private readonly SmartDecisionService _smartDecisionService;
    //    private readonly BuildCommandService _buildCommandService;
    //    private readonly IPickerService _pickerService;
    //    private readonly TranscodeQueueService _transcodeQueueService;
    //    private readonly SettingsService _settingsService;

    //    // ---------------------------------------------------------
    //    // CONSTRUCTOR
    //    // ---------------------------------------------------------
    //    public MainViewModel(
    //        ServerProfileService serverProfileService,
    //        FfprobeService ffprobeService,
    //        SmartDecisionService smartDecisionService,
    //        BuildCommandService buildCommandService,
    //        IPickerService pickerService,
    //        TranscodeQueueService transcodeQueueService,
    //        SettingsService settingsService)
    //    {
    //        _serverProfileService = serverProfileService;
    //        _ffprobeService = ffprobeService;
    //        _smartDecisionService = smartDecisionService;
    //        _buildCommandService = buildCommandService;
    //        _pickerService = pickerService;
    //        _transcodeQueueService = transcodeQueueService;
    //        _settingsService = settingsService;

    //        ServerProfiles = new ObservableCollection<ServerProfile>(_serverProfileService.GetAll());
    //        FinalCommand = string.Empty;

    //        SelectedServerProfile = ServerProfiles.FirstOrDefault(p => p.Name == settingsService.Settings.SelectedServerProfileName) ?? ServerProfiles.FirstOrDefault();
    //    }

    public MainViewModel()
    {

    }

    //    // ---------------------------------------------------------
    //    // ANALYZE FILE
    //    // ---------------------------------------------------------
    //    [RelayCommand]
    //    private async Task AnalyzeAsync()
    //    {
    //        if (SelectedFile == null)
    //            return;

    //        IsBusy = true;

    //        MediaInfo = await _ffprobeService.AnalyzeAsync(SelectedFile.Path, _settingsService.Settings);

    //        IsBusy = false;
    //    }

    //    // ---------------------------------------------------------
    //    // RUN SMART DECISION ENGINE
    //    // ---------------------------------------------------------
    //    [RelayCommand]
    //    private void GenerateDecision()
    //    {
    //        if (MediaInfo == null || SelectedServerProfile == null)
    //            return;

    //        Decision = _smartDecisionService.Evaluate(MediaInfo, SelectedServerProfile);
    //        DecisionExplanation = Decision?.Explanation ?? new();
    //        ConfidenceScore = Decision?.Confidence ?? 0;
    //        ConfidenceExplanation = Decision?.ConfidenceExplanation ?? [];
    //    }

    //    // ---------------------------------------------------------
    //    // BUILD TRANSCODE TASK
    //    // ---------------------------------------------------------
    //    [RelayCommand]
    //    private void BuildTask()
    //    {
    //        if (SelectedFile == null || MediaInfo == null || Decision == null || SelectedServerProfile == null)
    //            return;

    //        var outputPath = System.IO.Path.ChangeExtension(
    //            SelectedFile.Path,
    //            Decision.SelectedContainer
    //        );

    //        CurrentTask = new TranscodeTask
    //        {
    //            File = SelectedFile,
    //            Info = MediaInfo,
    //            Server = SelectedServerProfile,
    //            Decision = Decision,
    //            OutputPath = outputPath,
    //            DisplayName = $"{SelectedFile.Name} → {Decision.SelectedVideoCodec.ToUpper()}"
    //        };

    //        FinalCommand = _buildCommandService.Build(CurrentTask);
    //    }

    //    [RelayCommand]
    //    private async Task SelectFileAsync()
    //    {
    //        var fileInfo = await _pickerService.PickFileAsync();
    //        if (fileInfo == null) return;

    //        SelectedFile = new MediaFile
    //        {
    //            Path = fileInfo.Value.FilePath,
    //            SizeBytes = fileInfo.Value.Size
    //        };

    //        await AnalyzeAsync();
    //        GenerateDecision();
    //        BuildTask();
    //    }

    //    [RelayCommand(CanExecute = nameof(CanAddToQueue))]
    //    private async Task AddToQueue()
    //    {
    //        _transcodeQueueService.Add(SelectedFile!, MediaInfo!, Decision!, SelectedServerProfile!);
    //    }

    //    private async Task RunAnalysisPipelineAsync()
    //    {
    //        await AnalyzeAsync();
    //        GenerateDecision();
    //        BuildTask();
    //        AddToQueueCommand.NotifyCanExecuteChanged();
    //    }

    //    partial void OnSelectedFileChanged(MediaFile? value)
    //    {
    //        _ = RunAnalysisPipelineAsync();
    //    }

    //    partial void OnSelectedServerProfileChanged(ServerProfile? value)
    //    {
    //        _settingsService.Settings.SelectedServerProfileName = SelectedServerProfile?.Name ?? string.Empty;
    //        _settingsService.Save();
    //        _ = RunAnalysisPipelineAsync();
    //    }

    //    private bool CanAddToQueue()
    //    {
    //        return SelectedFile != null && SelectedServerProfile != null && Decision != null;
    //    }
}
