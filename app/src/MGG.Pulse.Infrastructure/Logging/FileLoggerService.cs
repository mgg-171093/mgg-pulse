using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.Ports;

namespace MGG.Pulse.Infrastructure.Logging;

public class FileLoggerService : ILoggerService
{
    private static readonly string LogDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MGG", "Pulse", "logs");

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public event Action<string>? LogEntryAdded;

    private string GetLogFilePath() =>
        Path.Combine(LogDirectory, $"pulse-{DateTime.Now:yyyy-MM-dd}.log");

    public async Task LogAsync(LogLevel level, string message, CancellationToken cancellationToken = default)
    {
        var entry = FormatEntry(level, message);
        LogEntryAdded?.Invoke(entry);

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(LogDirectory);
            await File.AppendAllTextAsync(GetLogFilePath(), entry + Environment.NewLine, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Log(LogLevel level, string message)
    {
        var entry = FormatEntry(level, message);
        LogEntryAdded?.Invoke(entry);

        _semaphore.Wait();
        try
        {
            Directory.CreateDirectory(LogDirectory);
            File.AppendAllText(GetLogFilePath(), entry + Environment.NewLine);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static string FormatEntry(LogLevel level, string message) =>
        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level.ToString().ToUpperInvariant()}] {message}";
}
