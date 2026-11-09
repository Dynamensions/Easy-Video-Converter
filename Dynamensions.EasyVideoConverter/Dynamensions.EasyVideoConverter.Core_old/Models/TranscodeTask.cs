using CommunityToolkit.Mvvm.ComponentModel;
using Dynamensions.EasyVideoConverter.Core.Models.Profiles;

namespace Dynamensions.EasyVideoConverter.Core.Models;

public partial class TranscodeTask : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime StartDateTime { get; set; }
    public PerformanceBucketKey PerformanceKey { get; } = new PerformanceBucketKey();


    // ---------------------------------------------------------
    // INPUTS
    // ---------------------------------------------------------
    public MediaFile File { get; set; } = default!;
    public MediaInfo Info { get; set; } = default!;
    public EncoderProfile Server { get; set; } = default!;

    // Output path (container determined by SmartDecision)
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OutputFileName))]
    private string outputPath = string.Empty;

    public string OutputFileName => Path.GetFileName(OutputPath);

    // ---------------------------------------------------------
    // DECISIONS (produced by SmartDecisionService)
    // ---------------------------------------------------------
    public SmartDecision Decision { get; set; } = new();

    // ---------------------------------------------------------
    // FINAL COMMAND
    // ---------------------------------------------------------
    public string FinalCommand { get; set; } = string.Empty;

    // ---------------------------------------------------------
    // METADATA
    // ---------------------------------------------------------
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Display name for UI (e.g., "MovieName (1080p → HEVC)")
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Optional: path to extracted subtitle file for burn-in.
    /// </summary>
    public string? BurnInSubtitlePath { get; set; }

    public required ClientProfile Client { get; set; }

    [ObservableProperty]
    private string? errorMessage;

}
