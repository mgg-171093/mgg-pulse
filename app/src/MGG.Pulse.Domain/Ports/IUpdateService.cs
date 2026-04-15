using MGG.Pulse.Domain.Updates;

namespace MGG.Pulse.Domain.Ports;

/// <summary>
/// Port for fetching update manifests from a remote source.
/// Implemented in Infrastructure — never referenced by Application directly,
/// only injected via this interface.
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Fetches the latest update manifest from the remote release source.
    /// </summary>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>The parsed <see cref="UpdateManifest"/> if available.</returns>
    /// <exception cref="System.Net.Http.HttpRequestException">Thrown when network fails.</exception>
    public Task<UpdateManifest> FetchManifestAsync(CancellationToken cancellationToken);
}
