using MGG.Pulse.Application.Rules;
using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.Ports;
using Moq;
using Xunit;

namespace MGG.Pulse.Tests.Unit.Application.Rules;

public class RuleEngineTests
{
    private readonly Mock<IIdleDetector> _mockDetector = new();

    private static SimulationContext BuildContext(SimulationMode mode = SimulationMode.Intelligent) =>
        new(mode, TimeSpan.Zero, DateTime.UtcNow);

    [Fact]
    public void Evaluate_WhenAllRulesPass_ShouldExecute()
    {
        _mockDetector.Setup(x => x.GetIdleTime()).Returns(TimeSpan.FromMinutes(2));

        var engine = new RuleEngine(new IRule[]
        {
            new IdleRule(_mockDetector.Object, thresholdSeconds: 30),
            new IntervalRule(minIntervalSeconds: 30, lastExecutionTime: DateTime.UtcNow.AddMinutes(-2))
        });

        var result = engine.Evaluate(BuildContext());

        Assert.True(result.ShouldExecute);
    }

    [Fact]
    public void Evaluate_WhenFirstRuleBlocks_ShouldNotExecute()
    {
        _mockDetector.Setup(x => x.GetIdleTime()).Returns(TimeSpan.FromSeconds(5)); // less than threshold

        var engine = new RuleEngine(new IRule[]
        {
            new IdleRule(_mockDetector.Object, thresholdSeconds: 30)
        });

        var result = engine.Evaluate(BuildContext());

        Assert.False(result.ShouldExecute);
    }

    [Fact]
    public void Evaluate_AggressiveMode_BypassesIdleRule()
    {
        _mockDetector.Setup(x => x.GetIdleTime()).Returns(TimeSpan.Zero); // user is active

        var engine = new RuleEngine(new IRule[]
        {
            new IdleRule(_mockDetector.Object, thresholdSeconds: 30),
            new AggressiveModeRule()
        });

        // IdleRule is first and would block, but in Aggressive mode it bypasses itself
        var result = engine.Evaluate(BuildContext(SimulationMode.Aggressive));

        Assert.True(result.ShouldExecute);
    }

    [Fact]
    public void Evaluate_EmptyRuleSet_ShouldExecute()
    {
        var engine = new RuleEngine(Array.Empty<IRule>());

        var result = engine.Evaluate(BuildContext());

        Assert.True(result.ShouldExecute);
        Assert.Equal("All rules passed", result.Reason);
    }

    [Fact]
    public void Constructor_WithNullRules_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new RuleEngine(null!));
    }
}
