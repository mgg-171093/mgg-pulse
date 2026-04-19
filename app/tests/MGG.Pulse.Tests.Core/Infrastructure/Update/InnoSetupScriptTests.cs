using Xunit;

namespace MGG.Pulse.Tests.Core.Infrastructure.Update;

public class InnoSetupScriptTests
{
    [Fact]
    public void RunEntry_WhenUpdaterUsesSilentInstall_DoesNotSkipRelaunchInSilentMode()
    {
        // Arrange
        var script = File.ReadAllText(ResolveBuildFilePath("pulse.iss"));

        // Act / Assert
        Assert.Contains("Flags: nowait postinstall", script, StringComparison.Ordinal);
        Assert.DoesNotContain("skipifsilent", script, StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveBuildFilePath(string fileName)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "build", fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException($"Unable to locate build file '{fileName}' from '{AppContext.BaseDirectory}'.");
    }
}
