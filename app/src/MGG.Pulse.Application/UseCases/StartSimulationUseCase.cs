using MGG.Pulse.Application.Common;
using MGG.Pulse.Application.Orchestration;
using MGG.Pulse.Domain.Entities;
using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.Ports;

namespace MGG.Pulse.Application.UseCases;

public class StartSimulationUseCase
{
    private readonly CycleOrchestrator _orchestrator;
    private readonly ILoggerService _logger;

    public StartSimulationUseCase(CycleOrchestrator orchestrator, ILoggerService logger)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SimulationSession>> ExecuteAsync(
        SimulationConfig config,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (cancellationToken.IsCancellationRequested)
            return Result<SimulationSession>.Fail("Cancellation requested before start.");

        var session = new SimulationSession();

        try
        {
            await _logger.LogAsync(LogLevel.Normal, $"Starting simulation. Mode: {config.Mode}, Input: {config.InputType}", cancellationToken);
            await _orchestrator.RunAsync(config, session, cancellationToken);
            session.End();
            return Result<SimulationSession>.Ok(session);
        }
        catch (OperationCanceledException)
        {
            session.End();
            return Result<SimulationSession>.Ok(session);
        }
        catch (Exception ex)
        {
            return Result<SimulationSession>.Fail($"Simulation failed: {ex.Message}");
        }
    }
}
