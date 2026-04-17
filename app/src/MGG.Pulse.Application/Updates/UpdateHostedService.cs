using MGG.Pulse.Application.Abstractions;

namespace MGG.Pulse.Application.Updates;

/// <summary>
/// Background service that runs a startup-deferred update check and a
/// 4-hour periodic check, using <see cref="CheckForUpdateUseCase"/>.
///
/// Implements a lightweight hosted-service lifecycle (Start/Stop) without
/// depending on Microsoft.Extensions.Hosting, keeping Application layer clean.
/// </summary>
public sealed class UpdateHostedService : IDisposable
{
    private const int MaxStartupAttempts = 3;
    private static readonly TimeSpan[] DefaultStartupRetryDelays =
    [
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(20),
        TimeSpan.FromSeconds(80)
    ];

    private readonly CheckForUpdateUseCase _checkForUpdateUseCase;
    private readonly ITimeProvider _timeProvider;
    private readonly TimeSpan _startupDelay;
    private readonly IReadOnlyList<TimeSpan> _startupRetryDelays;

    private CancellationTokenSource? _cts;
    private IDisposable? _periodicTimer;

    /// <summary>
    /// Fired when a newer version is found.
    /// Subscribers (e.g. App.xaml.cs) can show an update notification on the UI thread.
    /// </summary>
    public event Action<UpdateCheckResult>? UpdateAvailable;

    public UpdateHostedService(
        CheckForUpdateUseCase checkForUpdateUseCase,
        ITimeProvider timeProvider,
        TimeSpan? startupDelay = null,
        IReadOnlyList<TimeSpan>? startupRetryDelays = null)
    {
        _checkForUpdateUseCase = checkForUpdateUseCase
            ?? throw new ArgumentNullException(nameof(checkForUpdateUseCase));
        _timeProvider = timeProvider
            ?? throw new ArgumentNullException(nameof(timeProvider));
        _startupDelay = startupDelay ?? TimeSpan.FromSeconds(5);
        _startupRetryDelays = startupRetryDelays ?? DefaultStartupRetryDelays;

        if (_startupRetryDelays.Count == 0)
        {
            throw new ArgumentException("Startup retry delays cannot be empty.", nameof(startupRetryDelays));
        }

        if (_startupRetryDelays.Any(delay => delay < TimeSpan.Zero))
        {
            throw new ArgumentException("Startup retry delays cannot contain negative values.", nameof(startupRetryDelays));
        }
    }

    /// <summary>
    /// Starts the service: fires a deferred startup check and registers the 4-hour timer.
    /// Does NOT block — returns immediately.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _cts.Token;

        // Deferred startup check — fire-and-forget after a short delay so the
        // main window is visible before any update notification appears.
        _ = Task.Run(async () =>
        {
            await Task.Delay(_startupDelay, token).ConfigureAwait(false);
            await RunCheckSafeAsync(token, isStartupCheck: true).ConfigureAwait(false);
        }, token);

        // Periodic 4-hour timer
        _periodicTimer = _timeProvider.CreateTimer(
            period: TimeSpan.FromHours(4),
            callback: token => RunCheckSafeAsync(token, isStartupCheck: false),
            cancellationToken: token);

        return Task.CompletedTask;
    }

    /// <summary>Stops the service and disposes the periodic timer.</summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        _periodicTimer?.Dispose();
        _periodicTimer = null;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _periodicTimer?.Dispose();
    }

    // ─────────────────────────────────────────────────────────────────────────

    private async Task RunCheckSafeAsync(CancellationToken cancellationToken, bool isStartupCheck)
    {
        if (isStartupCheck)
        {
            await RunStartupCheckWithRetryAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        _ = await ExecuteSingleAttemptSafeAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task RunStartupCheckWithRetryAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxStartupAttempts; attempt++)
        {
            var shouldStopRetrying = await ExecuteSingleAttemptSafeAsync(cancellationToken).ConfigureAwait(false);
            if (shouldStopRetrying)
            {
                return;
            }

            if (attempt == MaxStartupAttempts)
            {
                return;
            }

            var retryDelay = GetRetryDelayForAttempt(attempt);
            await Task.Delay(retryDelay, cancellationToken).ConfigureAwait(false);
        }
    }

    private TimeSpan GetRetryDelayForAttempt(int attempt)
    {
        // attempt=1 -> first retry delay, attempt=2 -> second retry delay, etc.
        var retryIndex = Math.Clamp(attempt - 1, 0, _startupRetryDelays.Count - 1);
        return _startupRetryDelays[retryIndex];
    }

    private async Task<bool> ExecuteSingleAttemptSafeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _checkForUpdateUseCase
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);

            if (result.IsSuccess && result.Value?.UpdateAvailable == true)
            {
                UpdateAvailable?.Invoke(result.Value);
            }

            // Successful execution (with or without update) should stop startup retries.
            return result.IsSuccess;
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown — swallow silently
            return true;
        }
        catch
        {
            // Network errors, parse errors etc. — swallow silently, try again based on caller policy
            return false;
        }
    }
}
