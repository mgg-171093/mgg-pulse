using MGG.Pulse.Domain.Entities;
using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.Ports;
using MGG.Pulse.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MGG.Pulse.Infrastructure.Persistence;

public class JsonConfigRepository : IConfigRepository
{
    private static readonly string ConfigDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MGG", "Pulse");

    private static readonly string ConfigPath =
        Path.Combine(ConfigDirectory, "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<SimulationConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(ConfigPath))
                return SimulationConfig.Default;

            var json = await File.ReadAllTextAsync(ConfigPath, cancellationToken);
            var dto = JsonSerializer.Deserialize<ConfigDto>(json, JsonOptions);

            if (dto is null)
                return SimulationConfig.Default;

            return new SimulationConfig(
                mode: dto.Mode,
                interval: new IntervalRange(dto.IntervalMinSeconds, dto.IntervalMaxSeconds),
                inputType: dto.InputType,
                idleThresholdSeconds: dto.IdleThresholdSeconds,
                startWithWindows: dto.StartWithWindows,
                startMinimized: dto.StartMinimized,
                minimizeToTray: dto.MinimizeToTray,
                logLevel: dto.LogLevel);
        }
        catch
        {
            return SimulationConfig.Default;
        }
    }

    public async Task SaveAsync(SimulationConfig config, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        Directory.CreateDirectory(ConfigDirectory);

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
            LogLevel = config.LogLevel
        };

        var json = JsonSerializer.Serialize(dto, JsonOptions);
        await File.WriteAllTextAsync(ConfigPath, json, cancellationToken);
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
    }
}
