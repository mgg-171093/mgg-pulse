using MGG.Pulse.Domain.Enums;

namespace MGG.Pulse.Domain.Ports;

public interface ILoggerService
{
    Task LogAsync(LogLevel level, string message, CancellationToken cancellationToken = default);
    void Log(LogLevel level, string message);
}
