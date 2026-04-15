using MGG.Pulse.Application.Abstractions;

namespace MGG.Pulse.Infrastructure.Update;

/// <summary>
/// Real-time implementation of <see cref="ITimeProvider"/> using
/// <see cref="PeriodicTimer"/> from .NET 6+.
/// </summary>
public sealed class SystemTimeProvider : ITimeProvider
{
    /// <inheritdoc/>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    /// <inheritdoc/>
    public IDisposable CreateTimer(
        TimeSpan period,
        Func<CancellationToken, Task> callback,
        CancellationToken cancellationToken)
    {
        var timer = new PeriodicTimerHandle(period, callback, cancellationToken);
        timer.Start();
        return timer;
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Private implementation using PeriodicTimer (non-drifting)
    // ──────────────────────────────────────────────────────────────────────────

    private sealed class PeriodicTimerHandle : IDisposable
    {
        private readonly PeriodicTimer _timer;
        private readonly Func<CancellationToken, Task> _callback;
        private readonly CancellationToken _cancellationToken;
        private Task? _loopTask;

        public PeriodicTimerHandle(
            TimeSpan period,
            Func<CancellationToken, Task> callback,
            CancellationToken cancellationToken)
        {
            _timer             = new PeriodicTimer(period);
            _callback          = callback;
            _cancellationToken = cancellationToken;
        }

        public void Start()
        {
            _loopTask = RunLoopAsync();
        }

        private async Task RunLoopAsync()
        {
            try
            {
                while (await _timer.WaitForNextTickAsync(_cancellationToken).ConfigureAwait(false))
                {
                    await _callback(_cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown — exit loop silently
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
