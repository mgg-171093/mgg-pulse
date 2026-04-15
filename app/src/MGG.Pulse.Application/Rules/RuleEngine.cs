namespace MGG.Pulse.Application.Rules;

public class RuleEngine
{
    private readonly IReadOnlyList<IRule> _rules;

    public RuleEngine(IEnumerable<IRule> rules)
    {
        _rules = (rules ?? throw new ArgumentNullException(nameof(rules))).ToList();
    }

    public RuleResult Evaluate(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var rule in _rules)
        {
            var result = rule.Evaluate(context);
            if (!result.ShouldExecute)
            {
                return result; // first block wins
            }
        }

        return RuleResult.Allow("All rules passed");
    }
}
