using MGG.Pulse.Application.Rules;
using MGG.Pulse.Domain.Entities;
using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.Ports;
using MGG.Pulse.Domain.ValueObjects;

namespace MGG.Pulse.Application.Orchestration;

public class CycleOrchestrator
{
    private readonly RuleEngine _ruleEngine;
    private readonly IInputSimulator _inputSimulator;
    private readonly IIdleDetector _idleDetector;
    private readonly ILoggerService _logger;
    private readonly IntervalRule _intervalRule;
    private readonly Random _random = new();

    public event Action<SimulationAction>? ActionExecuted;
    public event Action<TimeSpan>? IdleTimeUpdated;
    public event Action<DateTime>? NextScheduledUpdated;

    public CycleOrchestrator(
        RuleEngine ruleEngine,
        IInputSimulator inputSimulator,
        IIdleDetector idleDetector,
        ILoggerService logger)
    {
        _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
        _inputSimulator = inputSimulator ?? throw new ArgumentNullException(nameof(inputSimulator));
        _idleDetector = idleDetector ?? throw new ArgumentNullException(nameof(idleDetector));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _intervalRule = new IntervalRule(1); // will be updated per config
    }

    public async Task RunAsync(SimulationConfig config, SimulationSession session, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(session);

        await _logger.LogAsync(LogLevel.Normal, "Simulation cycle started.", cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var delay = CalculateDelay(config.Interval);
                var nextAt = DateTime.UtcNow.Add(delay);
                NextScheduledUpdated?.Invoke(nextAt);

                await Task.Delay(delay, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                var idleTime = _idleDetector.GetIdleTime();
                IdleTimeUpdated?.Invoke(idleTime);

                var context = new SimulationContext(config.Mode, idleTime, DateTime.UtcNow);
                var ruleResult = _ruleEngine.Evaluate(context);

                if (!ruleResult.ShouldExecute)
                {
                    await _logger.LogAsync(LogLevel.Verbose, $"Skipped: {ruleResult.Reason}", cancellationToken);
                    continue;
                }

                await _inputSimulator.ExecuteAsync(config.InputType, cancellationToken);

                var action = new SimulationAction(config.InputType, DateTime.UtcNow, ruleResult.Reason);
                session.RecordAction(action);
                ActionExecuted?.Invoke(action);
                _intervalRule.RecordExecution();

                await _logger.LogAsync(LogLevel.Normal, $"Action executed: {config.InputType} — {ruleResult.Reason}", cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(LogLevel.Normal, $"Error in cycle: {ex.Message}", cancellationToken);
            }
        }

        await _logger.LogAsync(LogLevel.Normal, "Simulation cycle stopped.", CancellationToken.None);
    }

    private TimeSpan CalculateDelay(Domain.ValueObjects.IntervalRange interval)
    {
        var seconds = interval.IsFixed
            ? interval.MinSeconds
            : _random.Next(interval.MinSeconds, interval.MaxSeconds + 1);
        return TimeSpan.FromSeconds(seconds);
    }
}
