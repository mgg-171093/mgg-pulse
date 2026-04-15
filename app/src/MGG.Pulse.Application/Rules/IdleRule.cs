using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.Ports;

namespace MGG.Pulse.Application.Rules;

public class IdleRule : IRule
{
    private readonly IIdleDetector _idleDetector;
    private readonly int _thresholdSeconds;

    public IdleRule(IIdleDetector idleDetector, int thresholdSeconds = 30)
    {
        _idleDetector = idleDetector ?? throw new ArgumentNullException(nameof(idleDetector));
        _thresholdSeconds = thresholdSeconds > 0
            ? thresholdSeconds
            : throw new ArgumentException("Threshold must be greater than 0.", nameof(thresholdSeconds));
    }

    public RuleResult Evaluate(SimulationContext context)
    {
        // Aggressive mode bypasses this rule
        if (context.Mode == SimulationMode.Aggressive)
        {
            return RuleResult.Allow("Aggressive mode bypasses idle check");
        }

        var idleTime = _idleDetector.GetIdleTime();
        if (idleTime.TotalSeconds >= _thresholdSeconds)
        {
            return RuleResult.Allow($"Idle threshold exceeded: {idleTime.TotalSeconds:F0}s >= {_thresholdSeconds}s");
        }

        return RuleResult.Block($"User is active: idle time {idleTime.TotalSeconds:F0}s < threshold {_thresholdSeconds}s");
    }
}
