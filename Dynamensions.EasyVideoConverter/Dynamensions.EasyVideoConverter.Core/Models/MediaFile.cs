namespace Dynamensions.EasyVideoConverter.Core.Models;

public class MediaFile
{
    public string Path { get; set; } = string.Empty;

    public string FileName => System.IO.Path.GetFileName(Path);

    // File name (auto-calculated)
    public string Name => System.IO.Path.GetFileName(Path);

    // File name without extension
    public string BaseName => System.IO.Path.GetFileNameWithoutExtension(Path);

    // File extension
    public string Extension => System.IO.Path.GetExtension(Path);

    // Directory
    public string Directory => System.IO.Path.GetDirectoryName(Path) ?? string.Empty;

    // Optional: file size
    public long SizeBytes { get; set; }
}
