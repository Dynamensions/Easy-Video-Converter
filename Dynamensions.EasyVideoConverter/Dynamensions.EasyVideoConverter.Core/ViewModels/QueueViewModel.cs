using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dynamensions.EasyVideoConverter.Core.Helpers;
using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Models.Enums;
using Dynamensions.EasyVideoConverter.Core.Models.Profiles;
using Dynamensions.EasyVideoConverter.Core.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Dynamensions.EasyVideoConverter.Core.ViewModels;

public partial class QueueViewModel : ObservableObject
{
    private readonly CancellationTokenSource cancellationTokenSource = new();

    #region Queue Collection (Exposed to UI)
    // =========================================================
    // QUEUE COLLECTION (EXPOSED TO UI)
    // =========================================================
    public ReadOnlyObservableCollection<QueueItem> Items => _queue.Items;
    #endregion Queue Collection (Exposed to UI)

    #region Other Collections (Exposed to UI)
    // =========================================================
    // OTHER COLLECTIONS (EXPOSED TO UI)
    // =========================================================
    public ObservableCollection<EncoderProfile> EncoderProfiles { get; } = [];
    public ObservableCollection<ClientProfileGroup> ClientProfiles { get; } = [];
    #endregion Other Collections (Exposed to UI)

    #region Queue Status Properties (Bound to UI)
    // =========================================================
    // QUEUE STATUS PROPERTIES (BOUND TO UI)
    // =========================================================
    [ObservableProperty] private string? queueStatus;
    [ObservableProperty] private bool isConverting = false;
    [ObservableProperty] private int totalItems;
    [ObservableProperty] private int completedItems;
    [ObservableProperty] private int pendingItems;
    [ObservableProperty] private int runningItems;
    [ObservableProperty] private int failedItems;
    [ObservableProperty] private bool autoClearCompleted;
    [ObservableProperty] private EncoderProfile selectedEncoderProfile;
    [ObservableProperty] private ClientProfile selectedClientProfile;

    [ObservableProperty] private bool isProfileSelectionEnabled;
    [ObservableProperty] private TimeSpan estimatedTimeRemaining;
    [ObservableProperty] private DateTime estimatedDateTimeCompleted;
    [ObservableProperty] private TimeSpan elapsedTime;
    [ObservableProperty] private DateTime startDateTime;


    private bool HasItemsToRun =>
        _queue.Items.Any(i => !i.IsCompleted && !i.IsFailed && !i.IsRunning);
    private readonly Dictionary<string, string> filePatternPresets = Models.FilenamePatternPresets.Presets;
    #endregion

    public Action<(QueueItem item, double ProgressValue, double CurrentFPS, double SpeedX, double secondsRemaining)>? ProgressCallbackDispatcher { get; set; }
    public Action<(QueueItem item, int Current, int Total, SubtitleTrack SubtitleTrack)>? SubtitleCallback { get; set; }


    #region Dependencies (Services)
    // =========================================================
    // DEPENDENCIES (SERVICES)
    // =========================================================
    private readonly ILogger _logger;
    private readonly ITranscodeQueueService _queue;
    private readonly IMediaFileAnalyzerService _mediaFileAnalyzerService;
    private readonly ITranscodeService _transcodeService;
    private readonly ISubtitleExtractorService _subtitleExtractorService;
    private readonly ISmartDecisionService _smartDecisionService;
    private readonly IEncoderProfileService _serverProfileService;
    private readonly IClientProfileService _clientProfileService;
    private readonly IPickerService _pickerService;
    private readonly ISystemService _systemService;
    private readonly ISettingsService _settingsService;
    private readonly EncodeTimeEstimator _encodeTimeEstimator;
    private readonly SmartDecisionComplianceValidator _smartDecisionComplianceValidator;
    #endregion

    #region Constructor
    // =========================================================
    // CONSTRUCTOR
    // =========================================================
    public QueueViewModel(
        ILogger<QueueViewModel> logger,
        // Core Services
        ITranscodeQueueService queue,
        IMediaFileAnalyzerService mediaFileAnalyzerService,
        ITranscodeService transcodeService,
        ISubtitleExtractorService subtitleExtractorService,

        // Decision & Profile Services
        ISmartDecisionService smartDecisionService,
        IEncoderProfileService serverProfileService,
        IClientProfileService clientProfileService,

        // Utilities & Configuration
        ISystemService systemService,
        IPickerService pickerService,
        ISettingsService settingsService,
        EncodeTimeEstimator encodeTimeEstimator,
        SmartDecisionComplianceValidator smartDecisionComplianceValidator)
    {
        _logger = logger;
        _queue = queue;
        _mediaFileAnalyzerService = mediaFileAnalyzerService;
        _transcodeService = transcodeService;
        _subtitleExtractorService = subtitleExtractorService;
        _smartDecisionService = smartDecisionService;
        _serverProfileService = serverProfileService;
        _clientProfileService = clientProfileService;
        _systemService = systemService;
        _pickerService = pickerService;
        _settingsService = settingsService;
        _encodeTimeEstimator = encodeTimeEstimator;
        _smartDecisionComplianceValidator = smartDecisionComplianceValidator;

        // Load encoder profiles
        var caps = _systemService.GetEncoderCapabilities();
        foreach (var profile in _serverProfileService.GetAvailableProfiles(caps))
            EncoderProfiles.Add(profile);

        // Load client profiles
        foreach (var group in _clientProfileService.Profiles
            .GroupBy(x => x.Value.DeviceFamily)
            .OrderBy(x => x.Key))
            ClientProfiles.Add(new ClientProfileGroup()
            {
                Key = group.Key,
                Profiles = [.. group.Select(x => x.Value).OrderBy(x => x.Name)]
            });

        // Set default selections
        SelectedEncoderProfile = EncoderProfiles.FirstOrDefault(x => x.Name == _settingsService.Settings.SelectedEncoderProfileName) ?? EncoderProfiles.First();
        SelectedClientProfile = ClientProfiles
            .SelectMany(x => x.Profiles)
            .Where(x => x.Name == settingsService.Settings.SelectedClientProfileName)
            .FirstOrDefault()
            ??
            ClientProfiles.SelectMany(x => x.Profiles).First();

        IsProfileSelectionEnabled = true;

        settingsService.SettingsChanged += () =>
        {
            UpdateDecisionForAllItems();
        };
    }
    #endregion

    public void UpdateElapsedTime()
    {
        if (!IsConverting) return;
        ElapsedTime = DateTime.Now - StartDateTime;
    }

    public void UpdateProgress(QueueItem item, (double ProgressValue, double CurrentFPS, double SpeedX, double secondsRemaining) p)
    {
        item.Progress = p.ProgressValue * 100;
        item.EstimatedTimeRemaining = TimeSpan.FromSeconds(p.secondsRemaining);
        item.CurrentFPS = p.CurrentFPS;
        item.SpeedX = p.SpeedX;

        UpdateEstimatedConversionTime();
        EstimatedDateTimeCompleted = DateTime.Now + EstimatedTimeRemaining;
    }




    // =========================================================
    // PROCESS A SINGLE QUEUE ITEM
    // =========================================================
    private async Task ProcessItemAsync(QueueItem item)
    {
        item.IsRunning = true;
        item.Status = "Analyzing";
        item.Task.StartDateTime = DateTime.Now;

        var outputPath = Path.GetDirectoryName(item.Task.OutputPath);
        if (outputPath != null)
            Directory.CreateDirectory(outputPath);

        try
        {
            var decision = item.Task.Decision;

            // the number of steps.
            var currentStep = 1;
            var totalSteps = 2; // transcoding and validation are mandatory.
            if (decision.SubtitleHandling == SubtitleMode.BurnIn && decision.SelectedSubtitle != null) totalSteps++;
            if (decision.ExternalSubtitles.Count != 0) totalSteps++;

            //foreach (var s in item.Info.Subtitles)
            //{
            //    Console.WriteLine($"SUB:  idx={s.FfmpegIndex}, lang={s.Language}, title={s.Title}, codec={s.Codec}");
            //}


            // ---------------------------------------------------------
            // 1. Burn-in subtitle extraction
            // ---------------------------------------------------------
            if (decision.SubtitleHandling == SubtitleMode.BurnIn &&
                decision.SelectedSubtitle != null)
            {
                item.Status = $"Extracting burn-in subtitle ({currentStep++}/{totalSteps})";

                item.Task.BurnInSubtitlePath =
                    await _subtitleExtractorService.ExtractForBurnInAsync(item, ProgressCallbackDispatcher, cancellationTokenSource.Token);
            }

            // ---------------------------------------------------------
            // 2. External subtitle extraction
            // ---------------------------------------------------------
            if (decision.ExternalSubtitles.Count != 0)
            {
                item.Status = $"Extracting external subtitles ({currentStep++}/{totalSteps})";

                await _subtitleExtractorService.ExportAllAsync(
                    item,
                    decision.ExternalSubtitles,
                    ProgressCallbackDispatcher,
                    SubtitleCallback,
                    cancellationTokenSource.Token);
            }

            // ---------------------------------------------------------
            // 3. Run FFmpeg (burn-in + strip internal subs)
            // ---------------------------------------------------------

            if (item.Task.Decision.IsDirectPlay)
            {
                item.Status = $"Copying ({currentStep++}/{totalSteps})";
                var copyProgress = new Progress<double>();
                copyProgress.ProgressChanged += (s, d) =>
                {
                    ProgressCallbackDispatcher?.Invoke((item, d, 0, 0, 0));
                };

                await _systemService.CopyFileAsync(item.File.Path, item.Task.OutputPath, true, copyProgress, cancellationTokenSource.Token);
            }
            else
            {
                item.Status = $"Transcoding ({currentStep++}/{totalSteps})";
                await _transcodeService.TranscodeAsync(item, ProgressCallbackDispatcher);
            }

            // ---------------------------------------------------------
            // 4. Validate output
            // ---------------------------------------------------------
            item.Status = $"Validating ({currentStep++}/{totalSteps})";

            var result = await _smartDecisionComplianceValidator.ValidateAsync(
                item.Task.OutputPath,
                item.Task.Client,
                item.Task.Decision,
                cancellationTokenSource.Token);

            if (!result.IsCompliant)
            {
                item.IsCompliant = false;
                result.Reasons.ForEach(r => item.ComplianceErrors.Add(r));
                _logger.LogWarning("Output does not match SmartDecision: {result}", result);
            }
            else
            {
                item.IsCompliant = true;
                _logger.LogInformation("Output validated successfully.");
            }

            // ---------------------------------------------------------
            // 5. Output stats
            // ---------------------------------------------------------
            item.OutputSize = result.MediaFile.SizeBytes;

            // ---------------------------------------------------------
            // 6. Completed
            // ---------------------------------------------------------
            item.IsCompleted = true;
            item.IsConverted = true;
        }
        catch (OperationCanceledException)
        {
            item.Status = "Cancelled";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Conversion failed.");
            item.Status = "Failed";
            item.IsFailed = true;
        }
        finally
        {
            item.IsRunning = false;
            item.IsConverted = true;
        }
    }


    // =========================================================
    // BUILD TRANSCODE TASK (FFmpeg INPUT MODEL)
    // =========================================================
    private TranscodeTask BuildTask(MediaFile mediaFile, MediaInfo mediaInfo, EncoderProfile serverProfile, SmartDecision smartDecision)
    {
        var task = new TranscodeTask
        {
            File = mediaFile,
            Info = mediaInfo,
            Server = serverProfile,
            Decision = smartDecision,
            OutputPath = GenerateOutputPath(mediaFile, mediaInfo, smartDecision),
            Client = SelectedClientProfile!
        };

        task.PerformanceKey.InputCodec = mediaInfo.VideoCodec;
        task.PerformanceKey.OutputCodec = smartDecision.SelectedVideoCodec;
        task.PerformanceKey.Width = smartDecision.TargetWidth;
        task.PerformanceKey.Height = smartDecision.TargetHeight;
        task.PerformanceKey.BitDepth = smartDecision.TargetBitDepth;
        task.PerformanceKey.BurnIn = smartDecision.SubtitleHandling == SubtitleMode.BurnIn;
        return task;
    }

    // =========================================================
    // OUTPUT PATH GENERATION
    // =========================================================
    private string GenerateOutputPath(MediaFile file, MediaInfo mediaInfo, SmartDecision d)
    {
        var inputPath = file.Path;
        var originalDir = Path.GetDirectoryName(inputPath)!;

        var outputRoot = _settingsService.Settings.OutputFolder ??
                         Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);

        var relative = Path.GetRelativePath(Path.GetPathRoot(originalDir)!, originalDir);
        var finalDir = Path.Combine(outputRoot, relative);


        var name = Path.GetFileNameWithoutExtension(inputPath);
        var ext = $".{d.TargetContainer}";
        var finalName = $"{name}{ext}";

        var filePatternPreset = filePatternPresets[_settingsService.Settings.SelectedPatternPreset];
        finalName = FilenamePatternEngine.Apply(filePatternPreset, finalName, d.VideoSummary, d.AudioSummary, SelectedClientProfile, SelectedEncoderProfile, d.SubtitleHandling, d.SelectedSubtitle, d.ExternalSubtitles, mediaInfo);


        return Path.Combine(finalDir, $"{finalName}{ext}");
    }

    // =========================================================
    // START QUEUE (PROCESS ALL ITEMS)
    // =========================================================
    [RelayCommand(CanExecute = nameof(CanStartQueue))]
    private async Task StartQueueAsync()
    {
        // set the start time.
        StartDateTime = DateTime.Now;

        // queue up the first item.
        QueueItem? queueItem = _queue.Items.FirstOrDefault(i => !i.IsConverted);

        IsConverting = true;

        // runs through the queue until all items have ran.
        while (queueItem != null)
        {
            QueueStatus = $"Processing {queueItem.File.Name}";
            RunningItems = 1;

            await ProcessItemAsync(queueItem);

            queueItem.EstimatedTimeRemaining = DateTime.Now - queueItem.Task.StartDateTime;


            if (AutoClearCompleted)
                _queue.RemoveCompleted();

            // Update counters
            UpdateQueueStatistics();

            // queue up the next item; if there is any.
            queueItem = _queue.Items.FirstOrDefault(i => !i.IsConverted);
        }

        IsConverting = false;
        QueueStatus = null;
    }

    // =========================================================
    // ITEM CONTROL COMMANDS
    // =========================================================
    [RelayCommand] private void CancelItem(QueueItem item) { item.Cancellation.Cancel(); StartQueueCommand.NotifyCanExecuteChanged(); UpdateQueueStatistics(); UpdateEstimatedConversionTime(); }
    [RelayCommand] private void ClearCompleted() { _queue.RemoveCompleted(); StartQueueCommand.NotifyCanExecuteChanged(); UpdateQueueStatistics(); UpdateEstimatedConversionTime(); }
    [RelayCommand] private void ClearFailed() { _queue.RemoveFailed(); StartQueueCommand.NotifyCanExecuteChanged(); UpdateQueueStatistics(); UpdateEstimatedConversionTime(); }
    [RelayCommand] private void ClearAll() { CancelAll(); _queue.ClearAll(); StartQueueCommand.NotifyCanExecuteChanged(); UpdateQueueStatistics(); UpdateEstimatedConversionTime(); }
    [RelayCommand] private void RemoveItem(QueueItem item) { item.Cancellation.Cancel(); _queue.Remove(item); StartQueueCommand.NotifyCanExecuteChanged(); UpdateQueueStatistics(); UpdateEstimatedConversionTime(); }

    // =========================================================
    // ADD FOLDER → ANALYZE → DECIDE → QUEUE
    // =========================================================
    [RelayCommand]
    private async Task AddFolderAsync()
    {
        var files = await _pickerService.PickFolderFilesAsync();
        if (files == null) return;

        IsProfileSelectionEnabled = false;


        foreach (var (filePath, size) in files)
            await AddFileToQueueAsync(filePath, size);

        IsProfileSelectionEnabled = true;
        StartQueueCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task AddFileAsync()
    {
        var file = await _pickerService.PickFileAsync();
        if (file == null) return;
        await AddFileToQueueAsync(file.Value.FilePath, file.Value.Size);
        StartQueueCommand.NotifyCanExecuteChanged();
    }


    partial void OnSelectedClientProfileChanged(ClientProfile value)
    {
        if (value.Name != _settingsService.Settings.SelectedClientProfileName)
        {
            _settingsService.Settings.SelectedClientProfileName = value?.Name ?? "Roku";
            _settingsService.Save();
        }
    }
    partial void OnSelectedEncoderProfileChanged(EncoderProfile value)
    {
        if (value.Name != _settingsService.Settings.SelectedEncoderProfileName)
        {
            _settingsService.Settings.SelectedEncoderProfileName = value?.Name ?? "CPU";
            _settingsService.Save();
        }
    }

    private async Task AddFileToQueueAsync(string filePath, long size)
    {
        // 1. Build MediaFile
        var mediaFile = new MediaFile
        {
            Path = filePath,
            SizeBytes = size
        };

        // 2. Analyze media
        var info = await _mediaFileAnalyzerService.AnalyzeAsync(filePath, CancellationToken.None);

        // 3. SmartDecision
        var decision = _smartDecisionService.Decide(SelectedEncoderProfile, SelectedClientProfile, info);

        // 4. Estimate encoding time
        var key = EncodeTimeEstimator.CreatePerformanceKey(info, decision);
        var conversionSeconds = _encodeTimeEstimator.EstimatePreEncode(key, info.Duration.TotalSeconds);

        // 5. build the task.
        var task = BuildTask(mediaFile, info, SelectedEncoderProfile, decision);

        // 6. Add to queue
        // zeroing estimate until it can become more accurate.
        _queue.Add(task, 0);
        UpdateQueueStatistics();
        UpdateEstimatedConversionTime();
    }

    private void UpdateDecisionForAllItems()
    {
        IsProfileSelectionEnabled = false;

        foreach (var item in _queue.Items.Where(x => x.IsConverted == false || x.IsRunning == false))
        {
            _smartDecisionService.Decide(SelectedEncoderProfile, SelectedClientProfile, item.Info, item.Task.Decision);

            // estimate conversion time
            // 1. Build temporary PerformanceKey for estimation
            var perfKey = EncodeTimeEstimator.CreatePerformanceKey(item.Info, item.Task.Decision);

            // 2. Estimate encoding time
            var secondsRemaining = _encodeTimeEstimator.EstimatePreEncode(perfKey, item.Info.Duration.TotalSeconds);
            item.EstimatedTimeRemaining = TimeSpan.FromSeconds(secondsRemaining);

            item.Task.OutputPath = GenerateOutputPath(item.File, item.Info, item.Task.Decision);
        }

        UpdateEstimatedConversionTime();
        IsProfileSelectionEnabled = true;
    }

    private void UpdateQueueStatistics()
    {
        TotalItems = _queue.Items.Count;
        CompletedItems = _queue.Items.Count(i => i.IsCompleted);
        FailedItems = _queue.Items.Count(i => i.IsFailed);
        PendingItems = _queue.Items.Count(i => !i.IsCompleted && !i.IsFailed && !i.IsRunning);
        RunningItems = _queue.Items.Count(i => i.IsRunning);
    }

    private void UpdateEstimatedConversionTime()
    {
        var totalSeconds = _queue.Items.Sum(x => x.EstimatedTimeRemaining.TotalSeconds);
        EstimatedTimeRemaining = TimeSpan.FromSeconds(totalSeconds);
    }


    private bool CanStartQueue() => _queue.Items.Any();

    private void CancelAll()
    {
        foreach (var item in _queue.Items)
        {
            cancellationTokenSource.Cancel();
            item.Cancellation.Cancel();
        }
    }

}
