using MGG.Pulse.Domain.Ports;
using System.Diagnostics;
using System.Security.Cryptography;

namespace MGG.Pulse.Infrastructure.Update;

/// <summary>
/// Downloads an installer from a URL to the system %TEMP% folder and
/// computes its SHA-256 hash for integrity verification.
/// </summary>
public sealed class HttpFileDownloader : IFileDownloader
{
    private readonly HttpClient _httpClient;

    public HttpFileDownloader(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc/>
    public async Task<string> DownloadToTempAsync(string url, CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(new Uri(url).LocalPath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = "MGGPulse-Setup.exe";
        }

        var destPath = Path.Combine(Path.GetTempPath(), fileName);

        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream    = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await contentStream.CopyToAsync(fileStream, cancellationToken);

        return destPath;
    }

    /// <inheritdoc/>
    public async Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
