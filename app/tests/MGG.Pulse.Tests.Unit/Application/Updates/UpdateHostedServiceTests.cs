using MGG.Pulse.Application.Abstractions;
using MGG.Pulse.Application.Common;
using MGG.Pulse.Application.Updates;
using Moq;
using Xunit;

namespace MGG.Pulse.Tests.Unit.Application.Updates;

/// <summary>
/// RED phase: Tests for UpdateHostedService startup and periodic scheduling behavior.
/// Written BEFORE UpdateHostedService is implemented (Task 5.1).
/// </summary>
public class UpdateHostedServiceTests
{
    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private readonly Mock<CheckForUpdateUseCase> _mockUseCase;
    private readonly Mock<ITimeProvider> _mockTimeProvider;
    private readonly List<Func<CancellationToken, Task>> _registeredCallbacks = new();

    private static readonly Result<UpdateCheckResult> _noUpdateResult =
        Result<UpdateCheckResult>.Ok(UpdateCheckResult.NoUpdate());

    public UpdateHostedServiceTests()
    {
        var mockService = new Mock<MGG.Pulse.Domain.Ports.IUpdateService>();
        mockService
            .Setup(s => s.FetchManifestAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MGG.Pulse.Domain.Updates.UpdateManifest
            {
                Version = "1.0.0",
                Url     = "https://example.com/setup.exe",
                Sha256  = "abc123"
            });

        _mockUseCase     = new Mock<CheckForUpdateUseCase>(mockService.Object, "1.0.0");
        _mockTimeProvider = new Mock<ITimeProvider>();

        // Capture callbacks passed to CreateTimer so tests can invoke them manually
        _mockTimeProvider
            .Setup(t => t.CreateTimer(
                It.IsAny<TimeSpan>(),
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns((TimeSpan _, Func<CancellationToken, Task> cb, CancellationToken __) =>
            {
                _registeredCallbacks.Add(cb);
                return Mock.Of<IDisposable>();
            });

        _mockUseCase
            .Setup(u => u.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_noUpdateResult);
    }

    private UpdateHostedService BuildService() =>
        new(_mockUseCase.Object, _mockTimeProvider.Object, startupDelay: TimeSpan.Zero);

    // ─────────────────────────────────────────────────────────────────────────
    //  Startup behavior
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task StartAsync_SchedulesPeriodicTimerWithFourHourPeriod()
    {
        // Arrange
        var sut = BuildService();

        // Act
        await sut.StartAsync(CancellationToken.None);

        // Assert — timer was created with the 4-hour period
        _mockTimeProvider.Verify(
            t => t.CreateTimer(
                TimeSpan.FromHours(4),
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        await sut.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StartAsync_TriggersInitialCheckSoon()
    {
        // Arrange
        var sut = BuildService();

        // Act
        await sut.StartAsync(CancellationToken.None);

        // Allow the fire-and-forget startup check to run
        await Task.Delay(200);

        // Assert — use case was called at least once for the startup deferred check
        _mockUseCase.Verify(
            u => u.ExecuteAsync(It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);

        await sut.StopAsync(CancellationToken.None);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Periodic timer behavior
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PeriodicCallback_InvokesCheckForUpdate()
    {
        // Arrange
        var sut = BuildService();
        await sut.StartAsync(CancellationToken.None);

        // Allow the initial check to complete first
        await Task.Delay(100);

        var callsBefore = _mockUseCase
            .Invocations
            .Count(i => i.Method.Name == nameof(CheckForUpdateUseCase.ExecuteAsync));

        // Act — simulate the periodic timer firing by invoking the registered callback
        Assert.NotEmpty(_registeredCallbacks);
        await _registeredCallbacks[0](CancellationToken.None);

        // Assert — one more call was made
        _mockUseCase.Verify(
            u => u.ExecuteAsync(It.IsAny<CancellationToken>()),
            Times.AtLeast(callsBefore + 1));

        await sut.StopAsync(CancellationToken.None);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Error handling — check failure must NOT crash the service
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PeriodicCallback_WhenCheckFails_DoesNotThrow()
    {
        // Arrange
        _mockUseCase
            .Setup(u => u.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UpdateCheckResult>.Fail("Network error"));

        var sut = BuildService();
        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(100);

        // Act & Assert — periodic callback must not propagate exceptions
        Assert.NotEmpty(_registeredCallbacks);
        var exception = await Record.ExceptionAsync(
            () => _registeredCallbacks[0](CancellationToken.None));

        Assert.Null(exception);

        await sut.StopAsync(CancellationToken.None);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Lifecycle — StopAsync disposes the timer
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task StopAsync_DisposesPeriodicTimer()
    {
        // Arrange
        var mockDisposable = new Mock<IDisposable>();
        _mockTimeProvider
            .Setup(t => t.CreateTimer(
                It.IsAny<TimeSpan>(),
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns(mockDisposable.Object);

        var sut = BuildService();
        await sut.StartAsync(CancellationToken.None);

        // Act
        await sut.StopAsync(CancellationToken.None);

        // Assert — timer was disposed on stop
        mockDisposable.Verify(d => d.Dispose(), Times.Once);
    }
}
