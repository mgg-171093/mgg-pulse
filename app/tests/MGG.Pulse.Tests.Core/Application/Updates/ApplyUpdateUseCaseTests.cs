using MGG.Pulse.Application.Common;
using MGG.Pulse.Application.Updates;
using MGG.Pulse.Domain.Ports;
using Moq;
using Xunit;

namespace MGG.Pulse.Tests.Core.Application.Updates;

/// <summary>
/// RED phase: Tests for <see cref="ApplyUpdateUseCase"/>.
/// Covers download-to-TEMP, SHA-256 verification, and silent installer launch contract.
/// Written BEFORE ApplyUpdateUseCase and IInstallerLauncher exist.
/// </summary>
public class ApplyUpdateUseCaseTests
{
    private const string ValidSha256 = "aabbccdd1122334455667788aabbccdd1122334455667788aabbccdd11223344";
    private const string DownloadUrl = "https://example.com/MGGPulse-Setup-1.1.0.exe";

    private readonly Mock<IFileDownloader> _mockDownloader = new();
    private readonly Mock<IInstallerLauncher> _mockLauncher = new();

    private ApplyUpdateUseCase BuildUseCase() =>
        new(_mockDownloader.Object, _mockLauncher.Object);

    // ──────────────────────────────────────────────
    //  Happy path — download, verify, launch
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenHashMatches_LaunchesSilentInstallerAndReturnsSuccess()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), "MGGPulse-Setup-1.1.0.exe");
        _mockDownloader
            .Setup(d => d.DownloadToTempAsync(DownloadUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tempPath);
        _mockDownloader
            .Setup(d => d.ComputeSha256Async(tempPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ValidSha256);
        _mockLauncher
            .Setup(l => l.LaunchSilentAsync(tempPath, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var useCase = BuildUseCase();

        // Act
        var result = await useCase.ExecuteAsync(DownloadUrl, ValidSha256, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockLauncher.Verify(l => l.LaunchSilentAsync(tempPath, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ──────────────────────────────────────────────
    //  Hash mismatch — abort; do NOT launch
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenHashMismatches_ReturnsFailAndNeverLaunches()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), "MGGPulse-Setup-1.1.0.exe");
        var wrongHash = "0000000000000000000000000000000000000000000000000000000000000000";

        _mockDownloader
            .Setup(d => d.DownloadToTempAsync(DownloadUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tempPath);
        _mockDownloader
            .Setup(d => d.ComputeSha256Async(tempPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wrongHash);

        var useCase = BuildUseCase();

        // Act
        var result = await useCase.ExecuteAsync(DownloadUrl, ValidSha256, CancellationToken.None);

        // Assert — must FAIL and must NEVER call LaunchSilent
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("SHA-256", result.Error, StringComparison.OrdinalIgnoreCase);
        _mockLauncher.Verify(l => l.LaunchSilentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ──────────────────────────────────────────────
    //  Download failure — propagates as Fail result
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenDownloadThrows_ReturnsFailAndNeverLaunches()
    {
        // Arrange
        _mockDownloader
            .Setup(d => d.DownloadToTempAsync(DownloadUrl, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Download failed"));

        var useCase = BuildUseCase();

        // Act
        var result = await useCase.ExecuteAsync(DownloadUrl, ValidSha256, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Download failed", result.Error);
        _mockLauncher.Verify(l => l.LaunchSilentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ──────────────────────────────────────────────
    //  CanApply — pure static guard: determines whether
    //  a result carries enough data to trigger the handoff
    // ──────────────────────────────────────────────

    [Fact]
    public void CanApply_WhenResultHasUrlAndSha256_ReturnsTrue()
    {
        // Arrange
        var result = UpdateCheckResult.Available(
            version:  "1.1.0",
            url:      "https://example.com/MGGPulse-Setup-1.1.0.exe",
            sha256:   "aabbccdd1122334455667788aabbccdd1122334455667788aabbccdd11223344");

        // Act
        var canApply = ApplyUpdateUseCase.CanApply(result);

        // Assert
        Assert.True(canApply);
    }

    [Fact]
    public void CanApply_WhenResultHasNoUrl_ReturnsFalse()
    {
        // Arrange — NoUpdate result carries null DownloadUrl and null Sha256
        var result = UpdateCheckResult.NoUpdate();

        // Act
        var canApply = ApplyUpdateUseCase.CanApply(result);

        // Assert
        Assert.False(canApply);
    }

    [Fact]
    public void CanApply_WhenResultHasUrlButNoSha256_ReturnsFalse()
    {
        // Arrange — forge a result with URL but empty sha256
        var result = UpdateCheckResult.Available(
            version: "1.1.0",
            url:     "https://example.com/MGGPulse-Setup-1.1.0.exe",
            sha256:  string.Empty);

        // Act
        var canApply = ApplyUpdateUseCase.CanApply(result);

        // Assert
        Assert.False(canApply);
    }
}
