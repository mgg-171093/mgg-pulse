using MGG.Pulse.Infrastructure.Update;
using Xunit;

namespace MGG.Pulse.Tests.Core.Infrastructure.Update;

public class InnoSetupInstallerLauncherTests
{
    [Fact]
    public void CreateStartInfo_WhenCalled_UsesSilentAndRestartApplicationFlags()
    {
        // Arrange
        var installerPath = @"C:\temp\MGGPulse-Setup.exe";

        // Act
        var startInfo = InnoSetupInstallerLauncher.CreateStartInfo(installerPath);

        // Assert
        Assert.Equal(installerPath, startInfo.FileName);
        Assert.Equal("/SILENT /RESTARTAPPLICATION", startInfo.Arguments);
        Assert.True(startInfo.UseShellExecute);
    }
}
