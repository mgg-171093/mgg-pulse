using MGG.Pulse.Application.Rules;
using MGG.Pulse.Domain.Enums;
using Xunit;

namespace MGG.Pulse.Tests.Core.Application.Rules;

public class AggressiveModeRuleTests
{
    [Fact]
    public void Evaluate_WhenAggressiveMode_ShouldAllowExecution()
    {
        var rule = new AggressiveModeRule();
        var context = new SimulationContext(SimulationMode.Aggressive, TimeSpan.Zero, DateTime.UtcNow);

        var result = rule.Evaluate(context);

        Assert.True(result.ShouldExecute);
        Assert.Contains("Aggressive", result.Reason);
        Assert.Equal(10, result.Priority);
    }

    [Theory]
    [InlineData(SimulationMode.Intelligent)]
    [InlineData(SimulationMode.Manual)]
    public void Evaluate_WhenNotAggressiveMode_ShouldAllowButNotPrioritize(SimulationMode mode)
    {
        var rule = new AggressiveModeRule();
        var context = new SimulationContext(mode, TimeSpan.Zero, DateTime.UtcNow);

        var result = rule.Evaluate(context);

        // Rule is neutral — does not block
        Assert.True(result.ShouldExecute);
        Assert.Equal(0, result.Priority);
    }
}
