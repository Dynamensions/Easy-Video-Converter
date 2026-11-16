using Dynamensions.EasyVideoConverter.Core.Models;
using System;

namespace Dynamensions.EasyVideoConverter.Core.Services;

public interface ISettingsService
{
    public AppSettings Settings { get; }
    public event Action? SettingsChanged;
    public string FFMpegPath { get; }

    /// <summary>
    /// Saves the settings syncrounously.
    /// </summary>
    void Save();
}
