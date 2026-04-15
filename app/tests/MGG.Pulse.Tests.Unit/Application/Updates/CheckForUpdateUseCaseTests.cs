using MGG.Pulse.Application.Updates;
using MGG.Pulse.Domain.Ports;
using MGG.Pulse.Domain.Updates;
using Moq;
using Xunit;

namespace MGG.Pulse.Tests.Unit.Application.Updates;

/// <summary>
/// RED phase: Tests written before CheckForUpdateUseCase exists.
/// These tests MUST fail until Task 2.2 (GREEN) is implemented.
/// </summary>
public class CheckForUpdateUseCaseTests
{
    private const string CurrentVersion = "1.0.0";
    private const string NewerVersion   = "1.1.0";
    private const string SameVersion    = "1.0.0";
    private const string OlderVersion   = "0.9.0";

    private readonly Mock<IUpdateService> _mockUpdateService = new();

    private CheckForUpdateUseCase BuildUseCase() =>
        new(_mockUpdateService.Object, CurrentVersion);

    // ──────────────────────────────────────────────
    //  Happy path — newer version available
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenNewerVersionAvailable_ReturnsUpdateAvailableTrue()
    {
        // Arrange
        var manifest = new UpdateManifest
        {
            Version = NewerVersion,
            Url     = "https://example.com/setup.exe",
            Sha256  = "abc123"
        };
        _mockUpdateService
            .Setup(s => s.FetchManifestAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(manifest);

        var useCase = BuildUseCase();

        // Act
        var result = await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.UpdateAvailable);
        Assert.Equal(NewerVersion, result.Value.AvailableVersion);
    }

    // ──────────────────────────────────────────────
    //  Same version — no update
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenSameVersion_ReturnsUpdateAvailableFalse()
    {
        // Arrange
        var manifest = new UpdateManifest
        {
            Version = SameVersion,
            Url     = "https://example.com/setup.exe",
            Sha256  = "abc123"
        };
        _mockUpdateService
            .Setup(s => s.FetchManifestAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(manifest);

        var useCase = BuildUseCase();

        // Act
        var result = await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.UpdateAvailable);
    }

    // ──────────────────────────────────────────────
    //  Older version in manifest — no update
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenOlderVersionInManifest_ReturnsUpdateAvailableFalse()
    {
        // Arrange
        var manifest = new UpdateManifest
        {
            Version = OlderVersion,
            Url     = "https://example.com/setup.exe",
            Sha256  = "abc123"
        };
        _mockUpdateService
            .Setup(s => s.FetchManifestAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(manifest);

        var useCase = BuildUseCase();

        // Act
        var result = await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.UpdateAvailable);
    }

    // ──────────────────────────────────────────────
    //  Service throws — returns failure result
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenServiceThrows_ReturnsFailResult()
    {
        // Arrange
        _mockUpdateService
            .Setup(s => s.FetchManifestAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var useCase = BuildUseCase();

        // Act
        var result = await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Network error", result.Error);
    }

    // ──────────────────────────────────────────────
    //  Cancellation — propagates gracefully
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ReturnsFailResult()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockUpdateService
            .Setup(s => s.FetchManifestAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var useCase = BuildUseCase();

        // Act
        var result = await useCase.ExecuteAsync(cts.Token);

        // Assert
        Assert.False(result.IsSuccess);
    }

    // ──────────────────────────────────────────────
    //  Invalid manifest — use case rejects it
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenManifestVersionIsEmpty_ReturnsFailResult()
    {
        // Arrange — manifest with empty version (invalid per ManifestValidator)
        var manifest = new UpdateManifest { Version = "", Url = "https://example.com/setup.exe", Sha256 = "abc123" };
        _mockUpdateService
            .Setup(s => s.FetchManifestAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(manifest);

        var useCase = BuildUseCase();

        // Act
        var result = await useCase.ExecuteAsync(CancellationToken.None);

        // Assert — invalid manifest MUST return a Fail result, not a false UpdateAvailable
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("Version", result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_WhenManifestUrlIsEmpty_ReturnsFailResult()
    {
        // Arrange — manifest with empty url
        var manifest = new UpdateManifest { Version = "1.1.0", Url = "", Sha256 = "abc123" };
        _mockUpdateService
            .Setup(s => s.FetchManifestAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(manifest);

        var useCase = BuildUseCase();

        // Act
        var result = await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Url", result.Error);
    }

    // ──────────────────────────────────────────────
    //  Manual check — same logic, explicitly named
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ManualCheck_InvokesSameFlowAsAutomaticCheck()
    {
        // Arrange
        var manifest = new UpdateManifest
        {
            Version = NewerVersion,
            Url     = "https://example.com/setup.exe",
            Sha256  = "abc123"
        };
        _mockUpdateService
            .Setup(s => s.FetchManifestAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(manifest);

        var useCase = BuildUseCase();

        // Act — calling ExecuteAsync twice simulates "manual check" reusing the same flow
        var result1 = await useCase.ExecuteAsync(CancellationToken.None);
        var result2 = await useCase.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.Equal(result1.Value!.UpdateAvailable, result2.Value!.UpdateAvailable);
        Assert.Equal(result1.Value.AvailableVersion, result2.Value.AvailableVersion);

        _mockUpdateService.Verify(
            s => s.FetchManifestAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
}
