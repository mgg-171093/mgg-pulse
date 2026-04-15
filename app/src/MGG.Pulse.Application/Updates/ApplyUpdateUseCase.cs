using MGG.Pulse.Application.Common;
using MGG.Pulse.Domain.Ports;

namespace MGG.Pulse.Application.Updates;

/// <summary>
/// Use Case: Download an update installer to %TEMP%, verify its SHA-256 digest,
/// and launch it silently so Inno Setup can replace the running app.
///
/// The caller should exit the application after this returns success so the
/// installer can overwrite the running executable.
/// </summary>
public sealed class ApplyUpdateUseCase
{
    private readonly IFileDownloader _downloader;
    private readonly IInstallerLauncher _launcher;

    public ApplyUpdateUseCase(IFileDownloader downloader, IInstallerLauncher launcher)
    {
        _downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
        _launcher   = launcher   ?? throw new ArgumentNullException(nameof(launcher));
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="result"/> carries a non-empty
    /// <see cref="UpdateCheckResult.DownloadUrl"/> AND a non-empty
    /// <see cref="UpdateCheckResult.Sha256"/> — the minimum data required to
    /// perform a silent installer handoff.
    /// </summary>
    public static bool CanApply(UpdateCheckResult result)
        => !string.IsNullOrWhiteSpace(result.DownloadUrl)
        && !string.IsNullOrWhiteSpace(result.Sha256);

    /// <summary>
    /// Downloads the installer from <paramref name="downloadUrl"/> to %TEMP%,
    /// verifies its SHA-256 against <paramref name="expectedSha256"/>,
    /// then launches it with /SILENT.
    /// </summary>
    public async Task<Result<bool>> ExecuteAsync(
        string downloadUrl,
        string expectedSha256,
        CancellationToken cancellationToken)
    {
        try
        {
            var localPath = await _downloader
                .DownloadToTempAsync(downloadUrl, cancellationToken)
                .ConfigureAwait(false);

            var actualSha256 = await _downloader
                .ComputeSha256Async(localPath, cancellationToken)
                .ConfigureAwait(false);

            if (!string.Equals(actualSha256, expectedSha256, StringComparison.OrdinalIgnoreCase))
            {
                return Result<bool>.Fail(
                    $"SHA-256 mismatch: expected {expectedSha256}, got {actualSha256}");
            }

            await _launcher
                .LaunchSilentAsync(localPath, cancellationToken)
                .ConfigureAwait(false);

            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }
}
