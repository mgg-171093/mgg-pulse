using MGG.Pulse.Domain.Entities;

namespace MGG.Pulse.Domain.Ports;

public interface IConfigRepository
{
    Task<SimulationConfig> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(SimulationConfig config, CancellationToken cancellationToken = default);
}
