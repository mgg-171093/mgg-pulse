using MGG.Pulse.Domain.Enums;

namespace MGG.Pulse.Domain.Ports;

public interface ILoggerService
{
    public Task LogAsync(LogLevel level, string message, CancellationToken cancellationToken = default);
    public void Log(LogLevel level, string message);
}
