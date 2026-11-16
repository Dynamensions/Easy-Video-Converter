using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Dynamensions.EasyVideoConverter.Services;

internal class NoticeService : INoticeService
{
    public async Task<List<Notice>> GetNoticesAsync(CancellationToken cancellationToken)
    {
        List<Notice> notices = [];

        // FFmpeg
        notices.Add(new Notice
        {
            Title = "FFmpeg Notice",
            Content = await LoadNoticeAsync("ffmpeg.txt", cancellationToken)
        });

        return notices;
    }

    private static async Task<string> LoadNoticeAsync(string fileName, CancellationToken cancellationToken)
    {
        StorageFolder installedFolder = Package.Current.InstalledLocation;

        StorageFile file = await installedFolder.GetFileAsync(
            $@"Assets\Notices\{fileName}");

        return await FileIO.ReadTextAsync(file);
    }
}
