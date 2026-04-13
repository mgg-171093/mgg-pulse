using MGG.Pulse.Domain.Enums;

namespace MGG.Pulse.Domain.Ports;

public interface IInputSimulator
{
    Task ExecuteAsync(InputType inputType, CancellationToken cancellationToken);
}
