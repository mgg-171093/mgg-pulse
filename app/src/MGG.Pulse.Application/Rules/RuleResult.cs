namespace MGG.Pulse.Application.Rules;

public record RuleResult(bool ShouldExecute, string Reason, int Priority = 0)
{
    public static RuleResult Allow(string reason, int priority = 0) =>
        new(true, reason, priority);

    public static RuleResult Block(string reason, int priority = 0) =>
        new(false, reason, priority);
}
