namespace MGG.Pulse.Application.Rules;

public class IntervalRule : IRule
{
    private readonly int _minIntervalSeconds;
    private DateTime _lastExecutionTime;

    public IntervalRule(int minIntervalSeconds, DateTime? lastExecutionTime = null)
    {
        _minIntervalSeconds = minIntervalSeconds > 0
            ? minIntervalSeconds
            : throw new ArgumentException("MinIntervalSeconds must be greater than 0.", nameof(minIntervalSeconds));
        _lastExecutionTime = lastExecutionTime ?? DateTime.MinValue;
    }

    public RuleResult Evaluate(SimulationContext context)
    {
        var elapsed = (DateTime.UtcNow - _lastExecutionTime).TotalSeconds;
        if (elapsed >= _minIntervalSeconds)
        {
            return RuleResult.Allow($"Interval elapsed: {elapsed:F0}s >= {_minIntervalSeconds}s");
        }

        return RuleResult.Block($"Interval not elapsed: {elapsed:F0}s < {_minIntervalSeconds}s");
    }

    public void RecordExecution() => _lastExecutionTime = DateTime.UtcNow;
}
