using CommunityToolkit.Mvvm.ComponentModel;
using Dynamensions.EasyVideoConverter.Core.Models;
using Dynamensions.EasyVideoConverter.Core.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamensions.EasyVideoConverter.Core.ViewModels;

public partial class AboutViewModel(INoticeService noticeService, ILogger<AboutViewModel> logger) : ObservableObject
{
    public ObservableCollection<Notice> Notices = [];

    public async Task PrepareViewModelAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Loading notices.");
        Notices.Clear();

        var noticies = await noticeService.GetNoticesAsync(cancellationToken);
        noticies.ForEach(notice => Notices.Add(notice));
    }


}
