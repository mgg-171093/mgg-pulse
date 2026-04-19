using MGG.Pulse.Domain.Entities;
using MGG.Pulse.Domain.Enums;
using MGG.Pulse.Domain.ValueObjects;
using MGG.Pulse.Infrastructure.Persistence;
using Xunit;

namespace MGG.Pulse.Tests.Core.Infrastructure.Persistence;

public class JsonConfigRepositoryTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _configPath;

    public JsonConfigRepositoryTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "mgg-pulse-tests", Guid.NewGuid().ToString("N"));
        _configPath = Path.Combine(_tempDirectory, "config.json");
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public async Task SaveAsync_AndLoadAsync_RoundTripAppearanceTheme()
    {
        var repository = new JsonConfigRepository(_configPath);
        var config = new SimulationConfig(
            mode: SimulationMode.Intelligent,
            interval: new IntervalRange(30, 60),
            inputType: InputType.Mouse,
            idleThresholdSeconds: 45,
            startWithWindows: true,
            startMinimized: false,
            minimizeToTray: true,
            logLevel: LogLevel.Verbose);

        config.UpdateAppearanceTheme("Light");
        await repository.SaveAsync(config);

        var loaded = await repository.LoadAsync();

        Assert.Equal("Light", loaded.AppearanceTheme);
    }

    [Fact]
    public async Task SaveAsync_AndLoadAsync_RoundTripAutoAppearanceTheme()
    {
        var repository = new JsonConfigRepository(_configPath);
        var config = new SimulationConfig(
            mode: SimulationMode.Intelligent,
            interval: new IntervalRange(30, 60),
            inputType: InputType.Mouse,
            appearanceTheme: "Auto");

        await repository.SaveAsync(config);

        var loaded = await repository.LoadAsync();

        Assert.Equal("Auto", loaded.AppearanceTheme);
    }

    [Fact]
    public async Task LoadAsync_WhenAppearanceThemeIsInvalid_FallsBackToDark()
    {
        var repository = new JsonConfigRepository(_configPath);
        var json = """
        {
          "mode": "Intelligent",
          "intervalMinSeconds": 30,
          "intervalMaxSeconds": 60,
          "inputType": "Mouse",
          "idleThresholdSeconds": 30,
          "startWithWindows": false,
          "startMinimized": false,
          "minimizeToTray": true,
          "logLevel": "Normal",
          "appearanceTheme": "Sepia"
        }
        """;

        await File.WriteAllTextAsync(_configPath, json);

        var loaded = await repository.LoadAsync();

        Assert.Equal("Dark", loaded.AppearanceTheme);
    }

    [Fact]
    public async Task LoadAsync_WhenAppearanceThemeMissing_UsesDefaultDark()
    {
        var repository = new JsonConfigRepository(_configPath);
        var json = """
        {
          "mode": "Intelligent",
          "intervalMinSeconds": 30,
          "intervalMaxSeconds": 60,
          "inputType": "Mouse",
          "idleThresholdSeconds": 30,
          "startWithWindows": false,
          "startMinimized": false,
          "minimizeToTray": true,
          "logLevel": "Normal"
        }
        """;

        await File.WriteAllTextAsync(_configPath, json);

        var loaded = await repository.LoadAsync();

        Assert.Equal("Dark", loaded.AppearanceTheme);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
