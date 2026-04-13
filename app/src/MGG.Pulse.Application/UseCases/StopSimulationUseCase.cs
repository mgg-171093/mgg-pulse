using MGG.Pulse.Application.Common;
using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.Ports;

namespace MGG.Pulse.Application.UseCases;

public class StopSimulationUseCase
{
    private readonly ILoggerService _logger;

    public StopSimulationUseCase(ILoggerService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _logger.LogAsync(LogLevel.Normal, "Stopping simulation.", cancellationToken);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail($"Stop failed: {ex.Message}");
        }
    }
}
