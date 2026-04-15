using MGG.Pulse.Domain.Entities;

namespace MGG.Pulse.Domain.Ports;

public interface IConfigRepository
{
    public Task<SimulationConfig> LoadAsync(CancellationToken cancellationToken = default);
    public Task SaveAsync(SimulationConfig config, CancellationToken cancellationToken = default);
}
