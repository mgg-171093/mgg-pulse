namespace MGG.Pulse.Application.Rules;

public interface IRule
{
    public RuleResult Evaluate(SimulationContext context);
}
