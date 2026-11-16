using Dynamensions.EasyVideoConverter.Core.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamensions.EasyVideoConverter.Core.Services;

public interface INoticeService
{
    Task<List<Notice>> GetNoticesAsync(CancellationToken cancellationToken);
}
