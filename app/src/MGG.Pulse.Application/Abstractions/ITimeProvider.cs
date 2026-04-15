namespace MGG.Pulse.Application.Abstractions;

/// <summary>
/// Abstraction over time-related operations.
/// Allows tests to substitute a fake clock without real delays.
/// </summary>
public interface ITimeProvider
{
    /// <summary>Returns the current UTC time.</summary>
    public DateTimeOffset UtcNow { get; }

    /// <summary>
    /// Creates a periodic timer that fires after each <paramref name="period"/>.
    /// Callers MUST dispose the returned object.
    /// </summary>
    public IDisposable CreateTimer(TimeSpan period, Func<CancellationToken, Task> callback, CancellationToken cancellationToken);
}
