using MGG.Pulse.Application.Orchestration;
using MGG.Pulse.Application.Rules;
using MGG.Pulse.Application.UseCases;
using MGG.Pulse.Domain.Entities;
using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.Ports;
using MGG.Pulse.Domain.ValueObjects;
using Moq;
using Xunit;

namespace MGG.Pulse.Tests.Unit.Application.UseCases;

public class StartSimulationUseCaseTests
{
    private readonly Mock<IInputSimulator> _mockInput = new();
    private readonly Mock<IIdleDetector> _mockDetector = new();
    private readonly Mock<ILoggerService> _mockLogger = new();

    private SimulationConfig BuildConfig() =>
        new(SimulationMode.Intelligent, new IntervalRange(1, 1), InputType.Mouse);

    private StartSimulationUseCase BuildUseCase()
    {
        _mockDetector.Setup(x => x.GetIdleTime()).Returns(TimeSpan.FromMinutes(5));
        _mockLogger.Setup(x => x.LogAsync(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        var engine = new RuleEngine(new IRule[]
        {
            new IdleRule(_mockDetector.Object, thresholdSeconds: 30)
        });

        var orchestrator = new CycleOrchestrator(engine, _mockInput.Object, _mockDetector.Object, _mockLogger.Object);
        return new StartSimulationUseCase(orchestrator, _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelledImmediately_ReturnsSuccessWithEmptySession()
    {
        var useCase = BuildUseCase();
        var config = BuildConfig();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await useCase.ExecuteAsync(config, cts.Token);

        Assert.False(result.IsSuccess);
        Assert.Contains("Cancellation", result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullConfig_ThrowsArgumentNullException()
    {
        var useCase = BuildUseCase();
        using var cts = new CancellationTokenSource();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => useCase.ExecuteAsync(null!, cts.Token));
    }

    [Fact]
    public async Task ExecuteAsync_CancelsAfterShortTime_ReturnsSuccessfulSession()
    {
        _mockInput.Setup(x => x.ExecuteAsync(It.IsAny<InputType>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        var useCase = BuildUseCase();
        var config = BuildConfig();

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        var result = await useCase.ExecuteAsync(config, cts.Token);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value!.EndedAt);
    }
}
