using MGG.Pulse.Application.Abstractions;
using MGG.Pulse.Application.Common;
using MGG.Pulse.Application.Updates;
using Moq;
using Xunit;

namespace MGG.Pulse.Tests.Core.Application.Updates;

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

    private UpdateHostedService BuildService(
        TimeSpan? startupDelay = null,
        IReadOnlyList<TimeSpan>? startupRetryDelays = null) =>
        new(
            _mockUseCase.Object,
            _mockTimeProvider.Object,
            startupDelay: startupDelay ?? TimeSpan.Zero,
            startupRetryDelays: startupRetryDelays);

    private int CountExecuteCalls() =>
        _mockUseCase
            .Invocations
            .Count(i => i.Method.Name == nameof(CheckForUpdateUseCase.ExecuteAsync));

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 1500)
    {
        var started = DateTime.UtcNow;
        while (!condition() && DateTime.UtcNow - started < TimeSpan.FromMilliseconds(timeoutMs))
        {
            await Task.Delay(25);
        }
    }

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

    [Fact]
    public async Task StartAsync_WhenStartupCheckFailsTransiently_RetriesUntilSuccess()
    {
        // Arrange
        _mockUseCase
            .SetupSequence(u => u.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UpdateCheckResult>.Fail("transient-1"))
            .ReturnsAsync(Result<UpdateCheckResult>.Fail("transient-2"))
            .ReturnsAsync(_noUpdateResult);

        var sut = BuildService(startupRetryDelays: new[]
        {
            TimeSpan.Zero,
            TimeSpan.Zero,
            TimeSpan.Zero
        });

        // Act
        await sut.StartAsync(CancellationToken.None);
        await WaitUntilAsync(() => CountExecuteCalls() >= 3);

        // Assert
        Assert.Equal(3, CountExecuteCalls());

        await sut.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StartAsync_WhenRetrySucceedsWithNewerVersion_RaisesUpdateAvailableEvent()
    {
        // Arrange
        var updateResult = UpdateCheckResult.Available(
            version: "1.1.0",
            url: "https://example.com/setup.exe",
            sha256: "abc123");

        _mockUseCase
            .SetupSequence(u => u.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UpdateCheckResult>.Fail("transient-1"))
            .ReturnsAsync(Result<UpdateCheckResult>.Ok(updateResult));

        UpdateCheckResult? raisedResult = null;

        var sut = BuildService(startupRetryDelays: new[]
        {
            TimeSpan.Zero,
            TimeSpan.Zero,
            TimeSpan.Zero
        });
        sut.UpdateAvailable += result => raisedResult = result;

        // Act
        await sut.StartAsync(CancellationToken.None);
        await WaitUntilAsync(() => raisedResult is not null);

        // Assert
        Assert.NotNull(raisedResult);
        Assert.True(raisedResult!.UpdateAvailable);
        Assert.Equal("1.1.0", raisedResult.AvailableVersion);
        Assert.Equal(2, CountExecuteCalls());

        await sut.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StartAsync_WhenStartupCheckKeepsFailing_StopsAfterThreeAttempts()
    {
        // Arrange
        _mockUseCase
            .Setup(u => u.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UpdateCheckResult>.Fail("always-fail"));

        var sut = BuildService(startupRetryDelays: new[]
        {
            TimeSpan.Zero,
            TimeSpan.Zero,
            TimeSpan.Zero
        });

        // Act
        await sut.StartAsync(CancellationToken.None);
        await WaitUntilAsync(() => CountExecuteCalls() >= 3);
        await Task.Delay(150);

        // Assert
        Assert.Equal(3, CountExecuteCalls());

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

    [Fact]
    public async Task PeriodicCallback_WhenCheckFails_DoesSingleAttemptWithoutInternalRetry()
    {
        // Arrange
        _mockUseCase
            .Setup(u => u.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_noUpdateResult);

        var sut = BuildService(
            startupDelay: TimeSpan.FromHours(1),
            startupRetryDelays: new[] { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero });

        await sut.StartAsync(CancellationToken.None);

        _mockUseCase.Invocations.Clear();
        _mockUseCase
            .Setup(u => u.ExecuteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UpdateCheckResult>.Fail("periodic-fail"));

        // Act
        Assert.NotEmpty(_registeredCallbacks);
        await _registeredCallbacks[0](CancellationToken.None);
        await Task.Delay(100);

        // Assert
        Assert.Equal(1, CountExecuteCalls());

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
