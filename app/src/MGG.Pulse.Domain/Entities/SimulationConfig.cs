using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.ValueObjects;

namespace MGG.Pulse.Domain.Entities;

public class SimulationConfig
{
    public SimulationMode Mode { get; private set; }
    public IntervalRange Interval { get; private set; }
    public InputType InputType { get; private set; }
    public int IdleThresholdSeconds { get; private set; }
    public bool StartWithWindows { get; private set; }
    public bool StartMinimized { get; private set; }
    public bool MinimizeToTray { get; private set; }
    public LogLevel LogLevel { get; private set; }

    private SimulationConfig() { } // for deserializer

    public SimulationConfig(
        SimulationMode mode,
        IntervalRange interval,
        InputType inputType,
        int idleThresholdSeconds = 30,
        bool startWithWindows = false,
        bool startMinimized = false,
        bool minimizeToTray = true,
        LogLevel logLevel = LogLevel.Normal)
    {
        Mode = mode;
        Interval = interval ?? throw new ArgumentNullException(nameof(interval));
        InputType = inputType;
        IdleThresholdSeconds = idleThresholdSeconds > 0
            ? idleThresholdSeconds
            : throw new ArgumentException("IdleThresholdSeconds must be greater than 0.", nameof(idleThresholdSeconds));
        StartWithWindows = startWithWindows;
        StartMinimized = startMinimized;
        MinimizeToTray = minimizeToTray;
        LogLevel = logLevel;
    }

    public static SimulationConfig Default => new(
        mode: SimulationMode.Intelligent,
        interval: new IntervalRange(30, 60),
        inputType: InputType.Mouse);

    public void UpdateMode(SimulationMode mode) => Mode = mode;
    public void UpdateInterval(IntervalRange interval) => Interval = interval ?? throw new ArgumentNullException(nameof(interval));
    public void UpdateInputType(InputType inputType) => InputType = inputType;
    public void UpdateStealthOptions(bool startWithWindows, bool startMinimized, bool minimizeToTray)
    {
        StartWithWindows = startWithWindows;
        StartMinimized = startMinimized;
        MinimizeToTray = minimizeToTray;
    }
    public void UpdateLogLevel(LogLevel logLevel) => LogLevel = logLevel;
}
