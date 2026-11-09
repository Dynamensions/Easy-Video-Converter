using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace Dynamensions.EasyVideoConverter.Core.Models;

public sealed class LogLine
{
    public int Index { get; set; }
    public DateTimeOffset? Timestamp { get; init; }

    public string Level { get; init; } = "";

    public string Message { get; init; } = "";

    public string RawText { get; init; } = "";

    public Brush LevelBrush =>
        Level switch
        {
            "DBG" => new SolidColorBrush(Colors.Gray),
            "INF" => new SolidColorBrush(Colors.DodgerBlue),
            "WRN" => new SolidColorBrush(Colors.Orange),
            "ERR" => new SolidColorBrush(Colors.Red),
            "FTL" => new SolidColorBrush(Colors.DarkRed),
            _ => Application.Current.Resources["TextFillColorPrimaryBrush"] as Brush
            ?? new SolidColorBrush(Colors.Black)
        };

    public string TimestampText =>
        Timestamp?.ToString("yyyy-MM-dd HH:mm:ss.fff") ?? "";

    public Brush RowBackground =>
        Index % 2 == 0
        ? new SolidColorBrush(Colors.Transparent)
        : new SolidColorBrush(Color.FromArgb(20, 128, 128, 128));
}