namespace MGG.Pulse.Application.Rules;

public interface IRule
{
    RuleResult Evaluate(SimulationContext context);
}
