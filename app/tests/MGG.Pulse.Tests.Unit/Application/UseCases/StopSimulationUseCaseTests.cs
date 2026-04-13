using MGG.Pulse.Application.UseCases;
using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.Ports;
using Moq;
using Xunit;

namespace MGG.Pulse.Tests.Unit.Application.UseCases;

public class StopSimulationUseCaseTests
{
    private readonly Mock<ILoggerService> _mockLogger = new();

    [Fact]
    public async Task ExecuteAsync_Always_ReturnsSuccess()
    {
        _mockLogger.Setup(x => x.LogAsync(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        var useCase = new StopSimulationUseCase(_mockLogger.Object);

        var result = await useCase.ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task ExecuteAsync_LogsStopMessage()
    {
        _mockLogger.Setup(x => x.LogAsync(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        var useCase = new StopSimulationUseCase(_mockLogger.Object);

        await useCase.ExecuteAsync();

        _mockLogger.Verify(x =>
            x.LogAsync(LogLevel.Normal, "Stopping simulation.", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new StopSimulationUseCase(null!));
    }
}
