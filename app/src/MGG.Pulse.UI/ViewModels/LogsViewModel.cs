using CommunityToolkit.Mvvm.ComponentModel;
using MGG.Pulse.Infrastructure.Logging;

namespace MGG.Pulse.UI.ViewModels;

public partial class LogsViewModel : ObservableObject
{
    private const int MaxCharacters = 10_000;

    [ObservableProperty]
    private string _logText = string.Empty;

    public LogsViewModel(FileLoggerService logger)
    {
        logger.LogEntryAdded += OnLogEntryAdded;
    }

    private void OnLogEntryAdded(string entry)
    {
        if (string.IsNullOrWhiteSpace(entry))
        {
            return;
        }

        DispatcherQueueHolder.Enqueue(() =>
        {
            var current = LogText ?? string.Empty;
            var updated = string.IsNullOrWhiteSpace(current)
                ? entry
                : $"{entry}{Environment.NewLine}{current}";

            LogText = updated.Length > MaxCharacters
                ? updated[..MaxCharacters]
                : updated;
        });
    }

    private static class DispatcherQueueHolder
    {
        public static void Enqueue(Action update)
        {
            var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            if (dispatcher is not null)
            {
                _ = dispatcher.TryEnqueue(() => update());
                return;
            }

            update();
        }
    }
}
