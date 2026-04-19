using MGG.Pulse.Application.Rules;
using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.Ports;
using Moq;
using Xunit;

namespace MGG.Pulse.Tests.Core.Application.Rules;

public class IdleRuleTests
{
    private readonly Mock<IIdleDetector> _mockDetector = new();

    private SimulationContext BuildContext(SimulationMode mode = SimulationMode.Intelligent) =>
        new(mode, TimeSpan.Zero, DateTime.UtcNow);

    [Fact]
    public void Evaluate_WhenIdleTimeExceedsThreshold_ShouldAllowExecution()
    {
        _mockDetector.Setup(x => x.GetIdleTime()).Returns(TimeSpan.FromSeconds(60));
        var rule = new IdleRule(_mockDetector.Object, thresholdSeconds: 30);

        var result = rule.Evaluate(BuildContext());

        Assert.True(result.ShouldExecute);
        Assert.Contains("idle threshold exceeded", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Evaluate_WhenIdleTimeIsLessThanThreshold_ShouldBlockExecution()
    {
        _mockDetector.Setup(x => x.GetIdleTime()).Returns(TimeSpan.FromSeconds(10));
        var rule = new IdleRule(_mockDetector.Object, thresholdSeconds: 30);

        var result = rule.Evaluate(BuildContext());

        Assert.False(result.ShouldExecute);
    }

    [Fact]
    public void Evaluate_WhenAggressiveMode_ShouldBypassIdleCheck()
    {
        _mockDetector.Setup(x => x.GetIdleTime()).Returns(TimeSpan.Zero);
        var rule = new IdleRule(_mockDetector.Object, thresholdSeconds: 30);
        var context = BuildContext(SimulationMode.Aggressive);

        var result = rule.Evaluate(context);

        Assert.True(result.ShouldExecute);
        Assert.Contains("Aggressive", result.Reason);
        _mockDetector.Verify(x => x.GetIdleTime(), Times.Never);
    }

    [Fact]
    public void Constructor_WithZeroThreshold_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new IdleRule(_mockDetector.Object, thresholdSeconds: 0));
    }

    [Fact]
    public void Constructor_WithNullDetector_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new IdleRule(null!, thresholdSeconds: 30));
    }
}
