namespace MGG.Pulse.Domain.Ports;

/// <summary>
/// Port for launching a local installer executable silently.
/// Implemented in Infrastructure — decoupled from Application logic.
/// </summary>
public interface IInstallerLauncher
{
    /// <summary>
    /// Launches the installer at <paramref name="installerPath"/> with the <c>/SILENT</c> flag.
    /// The caller should exit the application after this returns so the installer can replace files.
    /// </summary>
    public Task LaunchSilentAsync(string installerPath, CancellationToken cancellationToken);
}
