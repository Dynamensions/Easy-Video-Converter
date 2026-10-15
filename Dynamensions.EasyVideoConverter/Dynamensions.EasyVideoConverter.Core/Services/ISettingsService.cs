using Dynamensions.EasyVideoConverter.Core.Models;

namespace Dynamensions.EasyVideoConverter.Core.Services;

public interface ISettingsService
{
    public AppSettings Settings { get; }
    public string FfMpegDirectory { get; }
    public event Action? SettingsChanged;

    /// <summary>
    /// Saves the settings syncrounously.
    /// </summary>
    void Save();
}
