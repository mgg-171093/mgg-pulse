using MGG.Pulse.Domain.Ports;
using System.Diagnostics;

namespace MGG.Pulse.Infrastructure.Update;

/// <summary>
/// Launches a local Inno Setup installer with <c>/SILENT /RESTARTAPPLICATION</c>.
/// The calling application should exit shortly after this returns so the
/// installer can overwrite running binaries.
/// </summary>
public sealed class InnoSetupInstallerLauncher : IInstallerLauncher
{
    public static ProcessStartInfo CreateStartInfo(string installerPath)
    {
        return new ProcessStartInfo
        {
            FileName        = installerPath,
            Arguments       = "/SILENT /RESTARTAPPLICATION",
            UseShellExecute = true   // required to launch an elevated Inno Setup installer
        };
    }

    /// <inheritdoc/>
    public Task LaunchSilentAsync(string installerPath, CancellationToken cancellationToken)
    {
        var startInfo = CreateStartInfo(installerPath);

        Process.Start(startInfo);
        return Task.CompletedTask;
    }
}
