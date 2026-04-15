using MGG.Pulse.Domain.Enums;

namespace MGG.Pulse.Domain.Ports;

public interface IInputSimulator
{
    public Task ExecuteAsync(InputType inputType, CancellationToken cancellationToken);
}
