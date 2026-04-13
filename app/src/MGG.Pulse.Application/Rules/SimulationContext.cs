using MGG.Pulse.Domain.Enums;

namespace MGG.Pulse.Application.Rules;

public record SimulationContext(
    SimulationMode Mode,
    TimeSpan IdleTime,
    DateTime LastExecutedAt
);
