using CommunityToolkit.Mvvm.ComponentModel;

namespace Dynamensions.EasyVideoConverter.Core.Models.Subtitles;

public partial class SubtitleLanguageItem : ObservableObject
{
    public required string Code { get; set; }
    public required string Name { get; set; }

    [ObservableProperty]
    private bool isSelected;
}