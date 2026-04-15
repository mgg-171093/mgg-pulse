using MGG.Pulse.Application.Common;
using MGG.Pulse.Domain.Ports;

namespace MGG.Pulse.Application.Updates;

/// <summary>
/// Use Case: Check whether a newer version is available via <see cref="IUpdateService"/>.
/// Can be called at startup, on the 4-hour periodic timer, or manually from About page.
/// Returns a <see cref="Result{UpdateCheckResult}"/> — never throws for expected failures.
/// </summary>
public class CheckForUpdateUseCase
{
    private readonly IUpdateService _updateService;
    private readonly string _installedVersion;

    public CheckForUpdateUseCase(IUpdateService updateService, string installedVersion)
    {
        _updateService    = updateService ?? throw new ArgumentNullException(nameof(updateService));
        _installedVersion = installedVersion ?? throw new ArgumentNullException(nameof(installedVersion));
    }

    /// <summary>
    /// Fetches the remote manifest, validates it, and compares with the installed version.
    /// </summary>
    public virtual async Task<Result<UpdateCheckResult>> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var manifest = await _updateService.FetchManifestAsync(cancellationToken);

            var validation = ManifestValidator.Validate(manifest);
            if (!validation.IsValid)
            {
                return Result<UpdateCheckResult>.Fail(
                    $"Manifest validation failed: {string.Join(", ", validation.Errors)}");
            }

            if (VersionComparer.IsNewer(manifest.Version, _installedVersion))
            {
                return Result<UpdateCheckResult>.Ok(
                    UpdateCheckResult.Available(manifest.Version, manifest.Url, manifest.Sha256, manifest.Notes));
            }

            return Result<UpdateCheckResult>.Ok(UpdateCheckResult.NoUpdate());
        }
        catch (Exception ex)
        {
            return Result<UpdateCheckResult>.Fail(ex.Message);
        }
    }
}
