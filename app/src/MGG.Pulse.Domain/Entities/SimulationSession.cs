using MGG.Pulse.Domain.ValueObjects;

namespace MGG.Pulse.Domain.Entities;

public class SimulationSession
{
    private readonly List<SimulationAction> _actions = new();

    public Guid Id { get; } = Guid.NewGuid();
    public DateTime StartedAt { get; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; private set; }
    public IReadOnlyList<SimulationAction> Actions => _actions.AsReadOnly();

    public void RecordAction(SimulationAction action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _actions.Add(action);
    }

    public void End()
    {
        if (EndedAt.HasValue)
        {
            throw new InvalidOperationException("Session has already ended.");
        }

        EndedAt = DateTime.UtcNow;
    }

    public TimeSpan? Duration => EndedAt.HasValue ? EndedAt.Value - StartedAt : null;
}
