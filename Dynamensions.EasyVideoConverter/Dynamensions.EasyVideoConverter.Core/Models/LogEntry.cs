using System;
using System.IO;

namespace Dynamensions.EasyVideoConverter.Core.Models;

public class LogEntry
{
    public string FilePath { get; set; } = "";
    public string FileName => Path.GetFileName(FilePath);
    public string Content { get; set; } = "";
    public DateTime Timestamp { get; set; }
}