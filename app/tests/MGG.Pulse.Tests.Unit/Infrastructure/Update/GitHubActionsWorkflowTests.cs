using Xunit;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace MGG.Pulse.Tests.Unit.Infrastructure.Update;

public class GitHubActionsWorkflowTests
{
    [Fact]
    public void CiWorkflow_UsesHostedRunnerSafePipeline_WithIntegrationFilter()
    {
        var ciWorkflow = File.ReadAllText(ResolveRepoFilePath(".github", "workflows", "ci.yml"));
        var normalized = NormalizeLineEndings(ciWorkflow);

        Assert.Contains("runs-on: windows-latest", normalized, StringComparison.Ordinal);
        Assert.Contains("push:", normalized, StringComparison.Ordinal);
        Assert.Contains("- develop", normalized, StringComparison.Ordinal);
        Assert.Contains("pull_request:", normalized, StringComparison.Ordinal);
        Assert.Contains("- main", normalized, StringComparison.Ordinal);
        Assert.Contains("dotnet restore app/MGG.Pulse.slnx", normalized, StringComparison.Ordinal);
        Assert.Contains("dotnet build app/MGG.Pulse.slnx --configuration Release --no-restore", normalized, StringComparison.Ordinal);
        Assert.Contains("dotnet test app/tests/MGG.Pulse.Tests.Unit/MGG.Pulse.Tests.Unit.csproj --configuration Release --no-build --filter \"Category!=Integration\"", normalized, StringComparison.Ordinal);
        Assert.Contains("permissions:\n  contents: read", normalized, StringComparison.Ordinal);
        Assert.DoesNotContain("actionlint", normalized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReleaseWorkflow_DefinesDryRunForPrsAndPublishPathForMain()
    {
        var releaseWorkflow = NormalizeLineEndings(File.ReadAllText(ResolveRepoFilePath(".github", "workflows", "release.yml")));

        Assert.Contains("pull_request:", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("push:", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("release-readiness:", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("if: github.event_name == 'pull_request'", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("pwsh -File .github/scripts/bump-version.ps1 -PropsPath app/Directory.Build.props -DryRun", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("pwsh -File .github/scripts/publish-release.ps1", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("-DryRun", releaseWorkflow, StringComparison.Ordinal);

        Assert.Contains("release:", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("github.actor != 'github-actions[bot]'", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("pwsh -File .github/scripts/bump-version.ps1 -PropsPath app/Directory.Build.props", releaseWorkflow, StringComparison.Ordinal);
    }

    [Fact]
    public void ReleaseWorkflow_UsesDeterministicScriptOutputs_AndNoInlineGitConfig()
    {
        var releaseWorkflow = NormalizeLineEndings(File.ReadAllText(ResolveRepoFilePath(".github", "workflows", "release.yml")));

        Assert.Contains("id: bump", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("${{ steps.bump.outputs.version }}", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("${{ steps.bump.outputs.tag }}", releaseWorkflow, StringComparison.Ordinal);
        Assert.DoesNotContain("git config user.name", releaseWorkflow, StringComparison.Ordinal);
        Assert.DoesNotContain("gh release create", releaseWorkflow, StringComparison.Ordinal);
    }

    [Fact]
    public void ReleaseScripts_DeclareDeterministicContracts_AndAtomicMutationOrder()
    {
        var bumpScript = NormalizeLineEndings(File.ReadAllText(ResolveRepoFilePath(".github", "scripts", "bump-version.ps1")));
        var publishScript = NormalizeLineEndings(File.ReadAllText(ResolveRepoFilePath(".github", "scripts", "publish-release.ps1")));

        Assert.Contains("EXIT CODE CONTRACT", bumpScript, StringComparison.Ordinal);
        Assert.Contains(".PARAMETER PropsPath", bumpScript, StringComparison.Ordinal);
        Assert.Contains("version", bumpScript, StringComparison.Ordinal);
        Assert.Contains("tag", bumpScript, StringComparison.Ordinal);

        Assert.Contains("EXIT CODE CONTRACT", publishScript, StringComparison.Ordinal);
        Assert.Contains(".PARAMETER DryRun", publishScript, StringComparison.Ordinal);
        Assert.Contains("gh release create", publishScript, StringComparison.Ordinal);
        Assert.Contains("[skip ci]", publishScript, StringComparison.Ordinal);
        Assert.Contains("[skip release]", publishScript, StringComparison.Ordinal);
        Assert.DoesNotContain("gh release create $Tag $LatestPath", publishScript, StringComparison.Ordinal);

        var releaseIndex = publishScript.IndexOf("gh release create", StringComparison.Ordinal);
        var latestIndex = publishScript.IndexOf("Set-Content -Path $LatestPath", StringComparison.Ordinal);
        var commitIndex = publishScript.IndexOf("commit -m", StringComparison.Ordinal);

        Assert.True(releaseIndex >= 0, "Expected gh release create in publish script.");
        Assert.True(latestIndex > releaseIndex, "latest.json mutation must happen only after release publish.");
        Assert.True(commitIndex > latestIndex, "git commit must happen after latest.json mutation.");
    }

    [Fact]
    public void PublishReleaseScript_UsesNativeExitChecking_ForGhAndGitCommands()
    {
        var publishScript = NormalizeLineEndings(File.ReadAllText(ResolveRepoFilePath(".github", "scripts", "publish-release.ps1")));

        Assert.Contains("function Invoke-NativeCommand", publishScript, StringComparison.Ordinal);
        Assert.Contains("$LASTEXITCODE", publishScript, StringComparison.Ordinal);
        Assert.Contains("if ($nativeExit -ne 0)", publishScript, StringComparison.Ordinal);

        Assert.Matches(new Regex("Invoke-NativeCommand\\s+-Command\\s+\\{\\s*gh release create", RegexOptions.CultureInvariant), publishScript);
        Assert.Matches(new Regex("Invoke-NativeCommand\\s+-Command\\s+\\{\\s*git add", RegexOptions.CultureInvariant), publishScript);
        Assert.Matches(new Regex("Invoke-NativeCommand\\s+-Command\\s+\\{\\s*git -c user.name=", RegexOptions.CultureInvariant), publishScript);
        Assert.Matches(new Regex("Invoke-NativeCommand\\s+-Command\\s+\\{\\s*git push origin main", RegexOptions.CultureInvariant), publishScript);
    }

    [Fact]
    public void PublishReleaseScript_MapsDeterministicExitCodes_ForKnownFailureModes()
    {
        var publishScript = NormalizeLineEndings(File.ReadAllText(ResolveRepoFilePath(".github", "scripts", "publish-release.ps1")));

        Assert.Contains("Exit-WithCode 20", publishScript, StringComparison.Ordinal);
        Assert.Contains("-FailureCode 21", publishScript, StringComparison.Ordinal);
        Assert.Contains("Exit-WithCode 22", publishScript, StringComparison.Ordinal);
        Assert.Contains("-FailureCode 23", publishScript, StringComparison.Ordinal);
        Assert.Contains("Exit-WithCode 24", publishScript, StringComparison.Ordinal);
        Assert.Contains("Exit-WithCode 25", publishScript, StringComparison.Ordinal);
    }

    [Fact]
    public void BumpVersionScript_Returns10_WhenPropsFileMissing()
    {
        var scriptPath = ResolveRepoFilePath(".github", "scripts", "bump-version.ps1");
        var result = ExecutePwshScript(scriptPath, "-PropsPath __missing__/Directory.Build.props");

        Assert.Equal(10, result.ExitCode);
        Assert.Contains("Props file not found", result.Stderr, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PublishReleaseScript_Returns24_WhenRequiredArgumentMissing()
    {
        var scriptPath = ResolveRepoFilePath(".github", "scripts", "publish-release.ps1");
        var result = ExecutePwshScript(scriptPath, "-Tag v1.2.3 -InstallerPath app/build/output/missing.exe -Repo owner/repo -CommitSha deadbeef");

        Assert.Equal(24, result.ExitCode);
        Assert.Contains("Missing required argument: Version", result.Stderr, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PublishReleaseScript_Returns20_WhenInstallerMissing()
    {
        var scriptPath = ResolveRepoFilePath(".github", "scripts", "publish-release.ps1");
        var result = ExecutePwshScript(scriptPath, "-Version 1.2.3 -Tag v1.2.3 -InstallerPath app/build/output/missing.exe -Repo owner/repo -CommitSha deadbeef");

        Assert.Equal(20, result.ExitCode);
        Assert.Contains("Installer not found", result.Stderr, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ThemeServiceTests_ClassifyHeadlessUnsafeTests_AsIntegration()
    {
        var tests = File.ReadAllText(ResolveRepoFilePath("app", "tests", "MGG.Pulse.Tests.Unit", "UI", "Services", "ThemeServiceTests.cs"));

        Assert.Contains("[Trait(\"Category\", \"Integration\")]", tests, StringComparison.Ordinal);
        Assert.Contains("local-only", tests, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UnitTestProject_AndReadme_DocumentTraitFilteringContract()
    {
        var testProject = File.ReadAllText(ResolveRepoFilePath("app", "tests", "MGG.Pulse.Tests.Unit", "MGG.Pulse.Tests.Unit.csproj"));
        var readme = File.ReadAllText(ResolveRepoFilePath("README.md"));

        Assert.Contains("Category!=Integration", testProject, StringComparison.Ordinal);
        Assert.Contains("Category!=Integration", readme, StringComparison.Ordinal);
        Assert.Contains("latest.json", readme, StringComparison.Ordinal);
        Assert.Contains("main", readme, StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveRepoFilePath(params string[] relativeSegments)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, Path.Combine(relativeSegments));
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException($"Unable to locate repo file '{Path.Combine(relativeSegments)}' from '{AppContext.BaseDirectory}'.");
    }

    private static string NormalizeLineEndings(string value)
        => value.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static ScriptExecutionResult ExecutePwshScript(string scriptPath, string arguments)
    {
        var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            WorkingDirectory = GetRepositoryRoot(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = $"-NoProfile -NonInteractive -File \"{scriptPath}\" {arguments}"
        };

        process.Start();
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new ScriptExecutionResult(process.ExitCode, stdout, stderr);
    }

    private static string GetRepositoryRoot()
    {
        var scriptDirectory = Path.GetDirectoryName(ResolveRepoFilePath(".github", "scripts", "publish-release.ps1"));
        return Path.GetFullPath(Path.Combine(scriptDirectory!, "..", ".."));
    }

    private readonly record struct ScriptExecutionResult(int ExitCode, string Stdout, string Stderr);
}
