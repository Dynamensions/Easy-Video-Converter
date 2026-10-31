using Dynamensions.EasyVideoConverter.Core.Models;

namespace Dynamensions.EasyVideoConverter.Core.Services;

public interface IMediaFileAnalyzerService
{
    /// <summary>
    /// Analyzes a file and creates a <see cref="MediaInfo"/> object.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the process.</param>
    /// <returns>A <see cref="MediaInfo"/> object.</returns>
    Task<MediaInfo> AnalyzeAsync(string filePath, CancellationToken cancellationToken);


}
