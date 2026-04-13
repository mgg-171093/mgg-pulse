using MGG.Pulse.Domain.Enums;

namespace MGG.Pulse.Domain.ValueObjects;

public sealed record SimulationAction(
    InputType InputType,
    DateTime ExecutedAt,
    string TriggeredByRule
);
