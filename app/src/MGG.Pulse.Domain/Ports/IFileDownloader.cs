namespace MGG.Pulse.Domain.Ports;

/// <summary>
/// Port for downloading installer files from a remote URL.
/// Implemented in Infrastructure — isolated from Application logic.
/// </summary>
public interface IFileDownloader
{
    /// <summary>
    /// Downloads a file from <paramref name="url"/> to the system temp directory.
    /// </summary>
    /// <returns>The full local path of the downloaded file.</returns>
    Task<string> DownloadToTempAsync(string url, CancellationToken cancellationToken);

    /// <summary>
    /// Computes the SHA-256 hex digest of a local file.
    /// </summary>
    Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken);
}
