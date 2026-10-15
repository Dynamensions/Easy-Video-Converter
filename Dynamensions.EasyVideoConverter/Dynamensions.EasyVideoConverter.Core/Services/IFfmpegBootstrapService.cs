using Dynamensions.EasyVideoConverter.Core.Models;

namespace Dynamensions.EasyVideoConverter.Core.Services;

public interface IFfmpegBootstrapService
{
    bool Verify();
    Task InitializeAsync(IProgress<PrepareProgress> progess, CancellationToken cancellationToken);
    string GetFfmpegPath();
    bool IsInitialized { get; }
}