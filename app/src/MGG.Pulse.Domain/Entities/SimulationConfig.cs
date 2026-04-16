using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.ValueObjects;

namespace MGG.Pulse.Domain.Entities;

public class SimulationConfig
{
    private const string DarkAppearanceTheme = "Dark";
    private const string LightAppearanceTheme = "Light";
    private const string AutoAppearanceTheme = "Auto";

    public SimulationMode Mode { get; private set; }
    public IntervalRange Interval { get; private set; }
    public InputType InputType { get; private set; }
    public int IdleThresholdSeconds { get; private set; }
    public bool StartWithWindows { get; private set; }
    public bool StartMinimized { get; private set; }
    public bool MinimizeToTray { get; private set; }
    public LogLevel LogLevel { get; private set; }
    public string AppearanceTheme { get; private set; }

    private SimulationConfig()
    {
        // for deserializer
        Interval = new IntervalRange(30, 60);
        AppearanceTheme = DarkAppearanceTheme;
    }

    public SimulationConfig(
        SimulationMode mode,
        IntervalRange interval,
        InputType inputType,
        int idleThresholdSeconds = 30,
        bool startWithWindows = false,
        bool startMinimized = false,
        bool minimizeToTray = true,
        LogLevel logLevel = LogLevel.Normal,
        string appearanceTheme = DarkAppearanceTheme)
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
        AppearanceTheme = NormalizeAppearanceTheme(appearanceTheme);
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
    public void UpdateAppearanceTheme(string appearanceTheme) => AppearanceTheme = NormalizeAppearanceTheme(appearanceTheme);

    private static string NormalizeAppearanceTheme(string? appearanceTheme)
    {
        if (string.Equals(appearanceTheme, AutoAppearanceTheme, StringComparison.OrdinalIgnoreCase))
        {
            return AutoAppearanceTheme;
        }

        return string.Equals(appearanceTheme, LightAppearanceTheme, StringComparison.OrdinalIgnoreCase)
            ? LightAppearanceTheme
            : DarkAppearanceTheme;
    }
}
