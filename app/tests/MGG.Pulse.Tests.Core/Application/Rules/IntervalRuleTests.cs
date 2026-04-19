using MGG.Pulse.Application.Rules;
using MGG.Pulse.Domain.Enums;
using Xunit;

namespace MGG.Pulse.Tests.Core.Application.Rules;

public class IntervalRuleTests
{
    private static SimulationContext BuildContext() =>
        new(SimulationMode.Intelligent, TimeSpan.Zero, DateTime.UtcNow);

    [Fact]
    public void Evaluate_WhenEnoughTimeHasPassed_ShouldAllowExecution()
    {
        // Last execution was 2 minutes ago
        var rule = new IntervalRule(minIntervalSeconds: 30, lastExecutionTime: DateTime.UtcNow.AddMinutes(-2));

        var result = rule.Evaluate(BuildContext());

        Assert.True(result.ShouldExecute);
    }

    [Fact]
    public void Evaluate_WhenIntervalNotElapsed_ShouldBlockExecution()
    {
        // Last execution was 5 seconds ago, min is 30
        var rule = new IntervalRule(minIntervalSeconds: 30, lastExecutionTime: DateTime.UtcNow.AddSeconds(-5));

        var result = rule.Evaluate(BuildContext());

        Assert.False(result.ShouldExecute);
        Assert.Contains("Interval not elapsed", result.Reason);
    }

    [Fact]
    public void Evaluate_WhenNoLastExecution_ShouldAllowExecution()
    {
        // DateTime.MinValue as last execution — always elapsed
        var rule = new IntervalRule(minIntervalSeconds: 30);

        var result = rule.Evaluate(BuildContext());

        Assert.True(result.ShouldExecute);
    }

    [Fact]
    public void RecordExecution_UpdatesLastExecutionTime()
    {
        var rule = new IntervalRule(minIntervalSeconds: 30);
        rule.RecordExecution();

        // Right after recording, interval should not be elapsed
        var result = rule.Evaluate(BuildContext());

        Assert.False(result.ShouldExecute);
    }

    [Fact]
    public void Constructor_WithZeroMinInterval_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new IntervalRule(0));
    }
}
