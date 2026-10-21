using Dynamensions.EasyVideoConverter.Core.Models;

namespace Dynamensions.EasyVideoConverter.Core.Services;

public interface IBootstrapService
{
    /// <summary>
    /// Verify all dependencies are present
    /// </summary>
    /// <returns>True if everything is present; otherwise false.</returns>
    bool Verify();

    /// <summary>
    /// Prepares the application by downloading any dependencies, including extraction.
    /// </summary>
    /// <param name="progess">The progress callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to stop the process</param>
    Task PrepareDependenciesAsync(IProgress<PrepareProgress> progess, CancellationToken cancellationToken);

    /// <summary>
    /// A <see cref="bool"/> that determines if the application is done with all dependencies.
    /// If this is false after the <see cref="PrepareDependenciesAsync"/> process has ran, there was an error,
    /// and the application should not continue.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Used to exit the application.
    /// </summary>
    Task ExitAppAsync();
}