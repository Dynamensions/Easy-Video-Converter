using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dynamensions.EasyVideoConverter.Core.Models.Profiles;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

namespace Dynamensions.EasyVideoConverter.Core.Models;

public partial class QueueItem : ObservableObject
{
    public required MediaFile File { get; set; }
    public required MediaInfo Info { get; set; }
    public required EncoderProfile ServerProfile { get; set; }
    public required TranscodeTask Task { get; set; }

    public Process? FfmpegProcess { get; set; }
    public CancellationTokenSource Cancellation { get; set; } = new();

    // ---------------------------------------------------------
    // STATS
    // ---------------------------------------------------------

    [ObservableProperty] private Enums.PlaybackMode playbackMode;

    [ObservableProperty] private string status = "Pending";

    [ObservableProperty] private double progress = 0;

    [ObservableProperty] private double currentFPS = 0;

    [ObservableProperty] private double speedX = 0;

    [ObservableProperty] private TimeSpan estimatedTimeRemaining = TimeSpan.Zero;

    [ObservableProperty] private bool isRunning;
    [ObservableProperty] private bool isCompleted;

    [ObservableProperty] private bool isFailed;
    [ObservableProperty] private bool isConverted;

    // Compliance Validation
    [ObservableProperty, NotifyPropertyChangedFor(nameof(ComplianceDisplay))] private bool isCompliant;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(SizeChange))] private long outputSize;

    public ObservableCollection<string> ComplianceErrors { get; } = [];
    public string? ComplianceErrorsString => ComplianceErrors.Count > 0 ? String.Join(Environment.NewLine, ComplianceErrors) : null;
    public string ComplianceDisplay => IsCompliant ? "Compliant" : "Not Compliant";

    public long SizeChange => OutputSize - File.SizeBytes;

    [RelayCommand]
    private void RemoveItem()
    {
        WeakReferenceMessenger.Default.Send(new Messages.ItemRemovedMessage(this));
    }

}
