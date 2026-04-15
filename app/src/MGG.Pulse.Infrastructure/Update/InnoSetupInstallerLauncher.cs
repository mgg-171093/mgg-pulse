using MGG.Pulse.Domain.Ports;
using System.Diagnostics;

namespace MGG.Pulse.Infrastructure.Update;

/// <summary>
/// Launches a local Inno Setup installer with the <c>/SILENT</c> flag.
/// The calling application should exit shortly after this returns so the
/// installer can overwrite running binaries.
/// </summary>
public sealed class InnoSetupInstallerLauncher : IInstallerLauncher
{
    /// <inheritdoc/>
    public Task LaunchSilentAsync(string installerPath, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName        = installerPath,
            Arguments       = "/SILENT",
            UseShellExecute = true   // required to launch an elevated Inno Setup installer
        };

        Process.Start(startInfo);
        return Task.CompletedTask;
    }
}
