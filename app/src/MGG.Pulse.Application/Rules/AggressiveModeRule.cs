using MGG.Pulse.Domain.Enums;

namespace MGG.Pulse.Application.Rules;

public class AggressiveModeRule : IRule
{
    public RuleResult Evaluate(SimulationContext context)
    {
        if (context.Mode == SimulationMode.Aggressive)
            return RuleResult.Allow("Aggressive mode: always execute", priority: 10);

        // Not aggressive — this rule is neutral (does not block)
        return RuleResult.Allow("Not aggressive mode, rule not applicable");
    }
}
