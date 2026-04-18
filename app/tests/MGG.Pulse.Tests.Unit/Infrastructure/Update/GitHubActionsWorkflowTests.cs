using Xunit;

namespace MGG.Pulse.Tests.Unit.Infrastructure.Update;

public class GitHubActionsWorkflowTests
{
    [Fact]
    public void CiWorkflow_UsesWindowsRunner_AndRunsValidationOnDevelopAndRelevantPrs()
    {
        var ciWorkflow = File.ReadAllText(ResolveRepoFilePath(".github", "workflows", "ci.yml"));
        var normalized = NormalizeLineEndings(ciWorkflow);

        Assert.Contains("runs-on: windows-latest", normalized, StringComparison.Ordinal);
        Assert.Contains("push:", normalized, StringComparison.Ordinal);
        Assert.Contains("- develop", normalized, StringComparison.Ordinal);
        Assert.Contains("pull_request:", normalized, StringComparison.Ordinal);
        Assert.Contains("- main", normalized, StringComparison.Ordinal);
        Assert.Contains("dotnet test", normalized, StringComparison.Ordinal);
        Assert.Contains("permissions:\n  contents: read", normalized, StringComparison.Ordinal);
    }

    [Fact]
    public void ReleaseWorkflow_DefinesReadinessAndRealReleaseJobs_WithLoopPrevention()
    {
        var releaseWorkflow = File.ReadAllText(ResolveRepoFilePath(".github", "workflows", "release.yml"));

        Assert.Contains("pull_request:", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("push:", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("release-readiness:", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("if: github.event_name == 'pull_request'", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("release:", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("github.actor != 'github-actions[bot]'", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("[skip release]", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("app/Directory.Build.props", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("app/build/latest.json", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("gh release create", releaseWorkflow, StringComparison.Ordinal);
        Assert.DoesNotContain("gh release create $tag app/build/latest.json", releaseWorkflow, StringComparison.Ordinal);
    }

    [Fact]
    public void ReleaseWorkflow_DefersMetadataPushUntilAfterBuildHashAndReleaseSucceed()
    {
        var releaseWorkflow = File.ReadAllText(ResolveRepoFilePath(".github", "workflows", "release.yml"));

        Assert.DoesNotContain("Commit and push version bump", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("Commit and push release metadata", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("git add app/Directory.Build.props app/build/latest.json", releaseWorkflow, StringComparison.Ordinal);

        var createReleaseIndex = releaseWorkflow.IndexOf("Create GitHub release and upload installer", StringComparison.Ordinal);
        var updateLatestIndex = releaseWorkflow.IndexOf("Update latest.json (raw-main model)", StringComparison.Ordinal);
        var commitPushIndex = releaseWorkflow.IndexOf("Commit and push release metadata", StringComparison.Ordinal);

        Assert.True(createReleaseIndex >= 0, "Expected release creation step to exist.");
        Assert.True(updateLatestIndex > createReleaseIndex, "Expected latest.json update to happen after release creation.");
        Assert.True(commitPushIndex > updateLatestIndex, "Expected metadata push to happen only after release and latest.json update.");
    }

    [Fact]
    public void RootReadme_DocumentsCiCdModelAndRawMainLatestJsonContract()
    {
        var readme = File.ReadAllText(ResolveRepoFilePath("README.md"));

        Assert.Contains(".github/workflows/ci.yml", readme, StringComparison.Ordinal);
        Assert.Contains(".github/workflows/release.yml", readme, StringComparison.Ordinal);
        Assert.Contains("raw", readme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("latest.json", readme, StringComparison.Ordinal);
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
}
