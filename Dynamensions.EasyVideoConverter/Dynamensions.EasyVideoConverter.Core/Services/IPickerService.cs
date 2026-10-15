namespace Dynamensions.EasyVideoConverter.Core.Services;

public interface IPickerService
{
    Task<(string FilePath, long Size)?> PickFileAsync();
    Task<string?> PickFolderAsync();
    Task<List<(string FolderPath, long Size)>?> PickFolderFilesAsync();
}
