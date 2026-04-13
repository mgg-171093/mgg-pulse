using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.ValueObjects;

namespace MGG.Pulse.Domain.Entities;

public class SimulationState
{
    public bool IsRunning { get; private set; }
    public TimeSpan CurrentIdleTime { get; private set; }
    public SimulationAction? LastAction { get; private set; }
    public DateTime? NextScheduledAt { get; private set; }

    public void Start() => IsRunning = true;

    public void Stop()
    {
        IsRunning = false;
        NextScheduledAt = null;
    }

    public void UpdateIdleTime(TimeSpan idleTime) => CurrentIdleTime = idleTime;

    public void RecordAction(SimulationAction action)
    {
        LastAction = action ?? throw new ArgumentNullException(nameof(action));
    }

    public void SetNextScheduled(DateTime nextAt) => NextScheduledAt = nextAt;
}
