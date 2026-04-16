using MGG.Pulse.Domain.Entities;
using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.Ports;
using MGG.Pulse.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MGG.Pulse.Infrastructure.Persistence;

public class JsonConfigRepository : IConfigRepository
{
    private readonly string _configDirectory;
    private readonly string _configPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonConfigRepository()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MGG",
            "Pulse",
            "config.json"))
    {
    }

    public JsonConfigRepository(string configPath)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));

        var directory = Path.GetDirectoryName(_configPath);
        _configDirectory = !string.IsNullOrWhiteSpace(directory)
            ? directory
            : throw new ArgumentException("Config path must include a directory.", nameof(configPath));
    }

    public async Task<SimulationConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                return SimulationConfig.Default;
            }

            var json = await File.ReadAllTextAsync(_configPath, cancellationToken);
            var dto = JsonSerializer.Deserialize<ConfigDto>(json, JsonOptions);

            if (dto is null)
            {
                return SimulationConfig.Default;
            }

            return new SimulationConfig(
                mode: dto.Mode,
                interval: new IntervalRange(dto.IntervalMinSeconds, dto.IntervalMaxSeconds),
                inputType: dto.InputType,
                idleThresholdSeconds: dto.IdleThresholdSeconds,
                startWithWindows: dto.StartWithWindows,
                startMinimized: dto.StartMinimized,
                minimizeToTray: dto.MinimizeToTray,
                logLevel: dto.LogLevel,
                appearanceTheme: dto.AppearanceTheme);
        }
        catch
        {
            return SimulationConfig.Default;
        }
    }

    public async Task SaveAsync(SimulationConfig config, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        Directory.CreateDirectory(_configDirectory);

        var dto = new ConfigDto
        {
            Mode = config.Mode,
            IntervalMinSeconds = config.Interval.MinSeconds,
            IntervalMaxSeconds = config.Interval.MaxSeconds,
            InputType = config.InputType,
            IdleThresholdSeconds = config.IdleThresholdSeconds,
            StartWithWindows = config.StartWithWindows,
            StartMinimized = config.StartMinimized,
            MinimizeToTray = config.MinimizeToTray,
            LogLevel = config.LogLevel,
            AppearanceTheme = config.AppearanceTheme
        };

        var json = JsonSerializer.Serialize(dto, JsonOptions);
        await File.WriteAllTextAsync(_configPath, json, cancellationToken);
    }

    private class ConfigDto
    {
        public SimulationMode Mode { get; set; } = SimulationMode.Intelligent;
        public int IntervalMinSeconds { get; set; } = 30;
        public int IntervalMaxSeconds { get; set; } = 60;
        public InputType InputType { get; set; } = InputType.Mouse;
        public int IdleThresholdSeconds { get; set; } = 30;
        public bool StartWithWindows { get; set; }
        public bool StartMinimized { get; set; }
        public bool MinimizeToTray { get; set; } = true;
        public LogLevel LogLevel { get; set; } = LogLevel.Normal;
        public string AppearanceTheme { get; set; } = "Dark";
    }
}
