# Skill: C# Conventions — MGG Pulse

## Trigger
Load this skill when writing ANY C# code in this project.

---

## Naming Conventions

```csharp
// Types, Properties, Methods, Events → PascalCase
public class SimulationConfig { }
public string CurrentMode { get; set; }
public void StartSimulation() { }

// Private fields → _camelCase
private readonly IIdleDetector _idleDetector;
private CancellationTokenSource _cts;

// Local variables, parameters → camelCase
var idleTime = await _idleDetector.GetIdleTimeAsync(cancellationToken);
public async Task ExecuteAsync(CancellationToken cancellationToken)

// Interfaces → prefix I
public interface IInputSimulator { }

// Constants → PascalCase (NOT ALL_CAPS)
public const int DefaultIntervalSeconds = 30;
```

---

## Project Layer Rules

### Domain (MGG.Pulse.Domain)
- ZERO external NuGet dependencies
- ZERO references to other projects in this solution
- Contains: Entities, Value Objects, Enums, Port interfaces (IXxx)
- Entities have private setters, use factory methods or constructors for invariants
- Value Objects are immutable, override Equals/GetHashCode

```csharp
// Entity — enforce invariants in constructor
public class SimulationConfig
{
    public SimulationMode Mode { get; private set; }
    public IntervalRange Interval { get; private set; }
    public InputType InputType { get; private set; }

    private SimulationConfig() { } // EF / deserializer ctor

    public SimulationConfig(SimulationMode mode, IntervalRange interval, InputType inputType)
    {
        Mode = mode;
        Interval = interval ?? throw new ArgumentNullException(nameof(interval));
        InputType = inputType;
    }
}

// Value Object — immutable, structural equality
public sealed record IntervalRange(int MinSeconds, int MaxSeconds)
{
    public IntervalRange(int minSeconds, int maxSeconds) : this(minSeconds, maxSeconds)
    {
        if (minSeconds <= 0) throw new ArgumentException("Min must be > 0", nameof(minSeconds));
        if (maxSeconds < minSeconds) throw new ArgumentException("Max must be >= Min", nameof(maxSeconds));
    }
}
```

### Application (MGG.Pulse.Application)
- Depends ONLY on Domain
- Use Cases return `Result<T>` — never throw for expected failures
- Always accept `CancellationToken` in async methods
- No UI concerns, no Win32 concerns

```csharp
// Result pattern for Use Cases
public record Result<T>(T? Value, bool IsSuccess, string? Error)
{
    public static Result<T> Ok(T value) => new(value, true, null);
    public static Result<T> Fail(string error) => new(default, false, error);
}

// Use Case signature pattern
public class StartSimulationUseCase
{
    private readonly IRuleEngine _ruleEngine;
    private readonly IIdleDetector _idleDetector;
    private readonly ILoggerService _logger;

    public StartSimulationUseCase(IRuleEngine ruleEngine, IIdleDetector idleDetector, ILoggerService logger)
    {
        _ruleEngine = ruleEngine;
        _idleDetector = idleDetector;
        _logger = logger;
    }

    public async Task<Result<SimulationSession>> ExecuteAsync(SimulationConfig config, CancellationToken cancellationToken)
    {
        // ...
    }
}
```

### Infrastructure (MGG.Pulse.Infrastructure)
- Implements Domain ports (IXxx interfaces)
- May reference Win32 P/Invoke, JSON libraries, file system
- NEVER referenced by Application or Domain

### UI (MGG.Pulse.UI)
- ViewModels inherit `ObservableObject` from CommunityToolkit.Mvvm
- Commands use `[RelayCommand]` attribute
- Observable properties use `[ObservableProperty]` attribute
- Composition Root lives in `App.xaml.cs`

---

## Async Patterns

```csharp
// Always propagate CancellationToken
public async Task<Result<bool>> ExecuteAsync(CancellationToken cancellationToken)
{
    await Task.Delay(interval, cancellationToken); // cancellable delay
    cancellationToken.ThrowIfCancellationRequested();
    // ...
}

// Never use .Result or .Wait() — always await
// Bad:
var result = someTask.Result;
// Good:
var result = await someTask;
```

---

## Dependency Injection

```csharp
// App.xaml.cs — Composition Root
private IServiceProvider ConfigureServices()
{
    var services = new ServiceCollection();

    // Domain — nothing to register (no implementations here)

    // Application
    services.AddSingleton<RuleEngine>();
    services.AddTransient<StartSimulationUseCase>();
    services.AddTransient<StopSimulationUseCase>();

    // Infrastructure
    services.AddSingleton<IInputSimulator, Win32InputSimulator>();
    services.AddSingleton<IIdleDetector, Win32IdleDetector>();
    services.AddSingleton<IConfigRepository, JsonConfigRepository>();
    services.AddSingleton<ILoggerService, FileLoggerService>();
    services.AddSingleton<ITrayService, SystemTrayService>();

    // UI / ViewModels
    services.AddTransient<MainViewModel>();

    return services.BuildServiceProvider();
}
```

---

## Error Handling

```csharp
// Use Result<T> for expected failures (never throw from Use Cases for business logic)
var result = await _startUseCase.ExecuteAsync(config, cancellationToken);
if (!result.IsSuccess)
{
    await _logger.LogWarningAsync(result.Error!);
    return;
}

// Throw only for programming errors / invariant violations (in constructors)
// Use ArgumentException, InvalidOperationException — not custom exceptions for V1
```

---

## File Organization

```
MGG.Pulse.Domain/
├── Entities/
│   ├── SimulationConfig.cs
│   ├── SimulationState.cs
│   └── SimulationSession.cs
├── ValueObjects/
│   ├── IntervalRange.cs
│   └── SimulationAction.cs
├── Enums/
│   ├── SimulationMode.cs
│   └── InputType.cs
└── Ports/
    ├── IInputSimulator.cs
    ├── IIdleDetector.cs
    ├── ILoggerService.cs
    ├── IConfigRepository.cs
    └── ITrayService.cs

MGG.Pulse.Application/
├── UseCases/
│   ├── StartSimulationUseCase.cs
│   └── StopSimulationUseCase.cs
├── Rules/
│   ├── IRule.cs
│   ├── IdleRule.cs
│   ├── AggressiveModeRule.cs
│   ├── IntervalRule.cs
│   └── RuleEngine.cs
├── Orchestration/
│   └── CycleOrchestrator.cs
└── Common/
    └── Result.cs
```
