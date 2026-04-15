using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MGG.Pulse.Application.Orchestration;
using MGG.Pulse.Application.UseCases;
using MGG.Pulse.Domain.Entities;
using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.Ports;
using MGG.Pulse.Domain.ValueObjects;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace MGG.Pulse.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly StartSimulationUseCase _startUseCase;
    private readonly StopSimulationUseCase _stopUseCase;
    private readonly IConfigRepository _configRepository;
    private readonly ITrayService _trayService;
    private readonly CycleOrchestrator _orchestrator;

    private CancellationTokenSource? _cts;
    private SimulationConfig _config = SimulationConfig.Default;
    private Task? _simulationTask;

    private Action<Domain.ValueObjects.SimulationAction>? _onActionExecuted;
    private Action<Domain.ValueObjects.SimulationAction>? _logHandler;
    private Action<TimeSpan>? _onIdleTimeUpdated;
    private Action<DateTime>? _onNextScheduledUpdated;

    [ObservableProperty]
    private bool _isRunning;

    /// <summary>Inverse of IsRunning — used for {x:Bind} visibility bindings (no ConverterParameter support).</summary>
    public bool IsNotRunning => !IsRunning;

    /// <summary>Color brush for the running status indicator dot.</summary>
    public SolidColorBrush StatusIndicatorBrush { get; private set; } =
        new SolidColorBrush(Color.FromArgb(0xFF, 0x2A, 0x2E, 0x45)); // inactive (#2A2E45)

    private static readonly SolidColorBrush _activeColor =
        new(Color.FromArgb(0xFF, 0x4C, 0xAF, 0x50)); // #4CAF50
    private static readonly SolidColorBrush _inactiveColor =
        new(Color.FromArgb(0xFF, 0x2A, 0x2E, 0x45)); // #2A2E45

    [ObservableProperty]
    private string _statusText = "Inactive";

    [ObservableProperty]
    private string _lastActionText = "—";

    [ObservableProperty]
    private string _idleTimeText = "0s";

    [ObservableProperty]
    private string _nextScheduledText = "—";

    [ObservableProperty]
    private string _selectedMode = "Intelligent";

    [ObservableProperty]
    private string _selectedInputType = "Mouse";

    [ObservableProperty]
    private int _intervalMin = 30;

    [ObservableProperty]
    private int _intervalMax = 60;

    [ObservableProperty]
    private bool _startWithWindows;

    [ObservableProperty]
    private bool _startMinimized;

    [ObservableProperty]
    private bool _minimizeToTray = true;

    [ObservableProperty]
    private string _logText = string.Empty;

    public Microsoft.UI.Dispatching.DispatcherQueue? DispatcherQueue { get; set; }

    /// <summary>Fired when the simulation starts and MinimizeToTray is enabled — window should hide itself.</summary>
    public event Action? HideWindowRequested;

    public MainViewModel(
        StartSimulationUseCase startUseCase,
        StopSimulationUseCase stopUseCase,
        IConfigRepository configRepository,
        ITrayService trayService,
        CycleOrchestrator orchestrator)
    {
        _startUseCase = startUseCase;
        _stopUseCase = stopUseCase;
        _configRepository = configRepository;
        _trayService = trayService;
        _orchestrator = orchestrator;
    }

    public async Task InitializeAsync()
    {
        _config = await _configRepository.LoadAsync();
        LoadConfigToProperties();
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        IsRunning = true;
        StatusText = "Active";
        _trayService.SetRunningState(true);
        _trayService.SetTooltip("MGG Pulse — Active");

        _onActionExecuted = UpdateLastAction;
        _logHandler = a => AddLogEntry($"{a.InputType} at {a.ExecutedAt:HH:mm:ss}");
        _onIdleTimeUpdated = UpdateIdleTime;
        _onNextScheduledUpdated = UpdateNextScheduled;

        _orchestrator.ActionExecuted += _onActionExecuted;
        _orchestrator.ActionExecuted += _logHandler;
        _orchestrator.IdleTimeUpdated += _onIdleTimeUpdated;
        _orchestrator.NextScheduledUpdated += _onNextScheduledUpdated;

        AddLogEntry("Simulation started.");

        if (MinimizeToTray)
        {
            HideWindowRequested?.Invoke();
        }

        _simulationTask = _startUseCase.ExecuteAsync(_config, _cts.Token)
            .ContinueWith(t =>
            {
                DispatcherQueue?.TryEnqueue(() =>
                {
                    IsRunning = false;
                    StatusText = "Inactive";
                    _trayService.SetRunningState(false);
                    _trayService.SetTooltip("MGG Pulse — Inactive");
                });
            });

        await Task.CompletedTask;
    }

    private bool CanStart() => !IsRunning;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private async Task StopAsync()
    {
        _cts?.Cancel();
        await _stopUseCase.ExecuteAsync();
        IsRunning = false;
        StatusText = "Inactive";
        _trayService.SetRunningState(false);

        if (_onActionExecuted is not null)
        {
            _orchestrator.ActionExecuted -= _onActionExecuted;
        }

        if (_logHandler is not null)
        {
            _orchestrator.ActionExecuted -= _logHandler;
        }

        if (_onIdleTimeUpdated is not null)
        {
            _orchestrator.IdleTimeUpdated -= _onIdleTimeUpdated;
        }

        if (_onNextScheduledUpdated is not null)
        {
            _orchestrator.NextScheduledUpdated -= _onNextScheduledUpdated;
        }

        _onActionExecuted = null;
        _logHandler = null;
        _onIdleTimeUpdated = null;
        _onNextScheduledUpdated = null;

        AddLogEntry("Simulation stopped.");
    }

    private bool CanStop() => IsRunning;

    partial void OnIsRunningChanged(bool value)
    {
        StartCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(IsNotRunning));
        StatusIndicatorBrush = value ? _activeColor : _inactiveColor;
        OnPropertyChanged(nameof(StatusIndicatorBrush));
    }

    [RelayCommand]
    private async Task SaveConfigAsync()
    {
        UpdateConfigFromProperties();
        await _configRepository.SaveAsync(_config);
    }

    public void AddLogEntry(string entry)
    {
        DispatcherQueue?.TryEnqueue(() =>
        {
            LogText = LogText.Length > 10000
                ? entry + Environment.NewLine
                : entry + Environment.NewLine + LogText;
        });
    }

    public void UpdateIdleTime(TimeSpan idleTime)
    {
        DispatcherQueue?.TryEnqueue(() =>
        {
            IdleTimeText = idleTime.TotalSeconds >= 60
                ? $"{(int)idleTime.TotalMinutes}m {idleTime.Seconds}s"
                : $"{(int)idleTime.TotalSeconds}s";
        });
    }

    public void UpdateNextScheduled(DateTime nextAt)
    {
        DispatcherQueue?.TryEnqueue(() =>
        {
            var diff = nextAt - DateTime.UtcNow;
            NextScheduledText = diff.TotalSeconds > 0 ? $"in {(int)diff.TotalSeconds}s" : "now";
        });
    }

    public void UpdateLastAction(Domain.ValueObjects.SimulationAction action)
    {
        DispatcherQueue?.TryEnqueue(() =>
        {
            LastActionText = $"{action.InputType} at {action.ExecutedAt:HH:mm:ss}";
        });
    }

    private void LoadConfigToProperties()
    {
        SelectedMode = _config.Mode.ToString();
        SelectedInputType = _config.InputType.ToString();
        IntervalMin = _config.Interval.MinSeconds;
        IntervalMax = _config.Interval.MaxSeconds;
        StartWithWindows = _config.StartWithWindows;
        StartMinimized = _config.StartMinimized;
        MinimizeToTray = _config.MinimizeToTray;
    }

    private void UpdateConfigFromProperties()
    {
        if (Enum.TryParse<SimulationMode>(SelectedMode, out var mode))
        {
            _config.UpdateMode(mode);
        }

        if (Enum.TryParse<InputType>(SelectedInputType, out var inputType))
        {
            _config.UpdateInputType(inputType);
        }

        var min = Math.Max(1, IntervalMin);
        var max = Math.Max(min, IntervalMax);
        _config.UpdateInterval(new IntervalRange(min, max));
        _config.UpdateStealthOptions(StartWithWindows, StartMinimized, MinimizeToTray);
    }
}
