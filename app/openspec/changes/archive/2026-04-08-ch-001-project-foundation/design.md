# Design: MGG Pulse — Project Foundation

## Technical Approach

Construcción bottom-up en capas estrictamente aisladas. Cada capa se construye y verifica antes de la siguiente. El Domain define contratos (ports), Application consume los ports, Infrastructure los implementa, y UI los orquesta vía DI en App.xaml.cs.

## Architecture Decisions

### Decision: WinForms NotifyIcon para System Tray

| Opción | Trade-off |
|--------|-----------|
| WinForms `NotifyIcon` | Maduro, estable, bien documentado para .NET 8 |
| WinUI 3 nativo | API inestable en Windows App SDK ≤1.5, bugs conocidos |
| Win32 Shell_NotifyIcon directo | P/Invoke complejo, más surface de error |

**Elección**: WinForms `NotifyIcon` en thread STA dedicado.
**Rationale**: `NotifyIcon` es el estándar de .NET para tray. Correrlo en un thread STA separado del UI thread de WinUI 3 es un patrón probado. La sincronización se hace vía `DispatcherQueue`.

### Decision: Result<T> pattern sin librería externa

**Elección**: `Result<T>` propio en `MGG.Pulse.Application.Common`
**Alternativas**: `FluentResults`, `ErrorOr`, `OneOf`
**Rationale**: Domain debe tener zero dependencias. Un record simple de 5 líneas evita dependencias externas innecesarias para V1.

### Decision: CommunityToolkit.Mvvm para ViewModels

**Elección**: `[ObservableProperty]` y `[RelayCommand]` de CommunityToolkit.Mvvm
**Alternativas**: MVVM manual, Prism
**Rationale**: Source generators de CommunityToolkit eliminan boilerplate. Es el estándar de facto para WinUI 3 + MVVM.

### Decision: System.Text.Json para config (sin atributos en Domain)

**Elección**: Serialización/deserialización en `JsonConfigRepository` (Infrastructure), con JsonSerializerOptions configurado ahí.
**Rationale**: Domain no debe tener atributos `[JsonPropertyName]`. El adapter maneja el mapping.

## Data Flow

### Simulation Cycle

```
CycleOrchestrator.RunAsync()
    │
    ├── await Task.Delay(randomizedInterval, ct)
    │
    ├── RuleEngine.Evaluate(SimulationContext)
    │       ├── IdleRule.Evaluate()  ──→ IIdleDetector.GetIdleTime()
    │       ├── IntervalRule.Evaluate()
    │       └── AggressiveModeRule.Evaluate()
    │              └── RuleResult { ShouldExecute, Reason, Priority }
    │
    ├── [if ShouldExecute] IInputSimulator.ExecuteAsync(InputType, ct)
    │
    └── ILoggerService.LogAsync(action, ct)
```

### DI Wiring (App.xaml.cs)

```
App.xaml.cs (Composition Root)
    │
    ├── Register: IIdleDetector → Win32IdleDetector
    ├── Register: IInputSimulator → Win32InputSimulator
    ├── Register: IConfigRepository → JsonConfigRepository
    ├── Register: ILoggerService → FileLoggerService
    ├── Register: ITrayService → SystemTrayService
    ├── Register: RuleEngine
    ├── Register: CycleOrchestrator
    ├── Register: StartSimulationUseCase
    ├── Register: StopSimulationUseCase
    └── Register: MainViewModel
```

### Startup Flow

```
App.OnLaunched()
    │
    ├── Build DI container
    ├── Show SplashWindow
    │       ├── Animate logo (fade-in 800ms)
    │       ├── Init: IConfigRepository.LoadAsync()  → progress 33%
    │       ├── Init: ITrayService.Initialize()       → progress 66%
    │       ├── Init: FileLoggerService               → progress 100%
    │       └── Close SplashWindow
    │
    └── [if !StartMinimized] Show MainWindow
        [if StartMinimized]  Stay in tray
```

## File Changes

| File | Acción | Descripción |
|------|--------|-------------|
| `MGG.Pulse.sln` | Create | Solución con 5 proyectos |
| `src/MGG.Pulse.Domain/*.csproj` | Create | Domain — zero deps |
| `src/MGG.Pulse.Application/*.csproj` | Create | Referencia Domain |
| `src/MGG.Pulse.Infrastructure/*.csproj` | Create | Referencia Domain + WinForms |
| `src/MGG.Pulse.UI/*.csproj` | Create | WinUI 3, referencia Application + Infrastructure |
| `tests/MGG.Pulse.Tests.Unit/*.csproj` | Create | xUnit + Moq, referencia Domain + Application |
| `src/MGG.Pulse.Domain/Entities/` | Create | SimulationConfig, SimulationState, SimulationSession |
| `src/MGG.Pulse.Domain/ValueObjects/` | Create | IntervalRange, SimulationAction |
| `src/MGG.Pulse.Domain/Enums/` | Create | SimulationMode, InputType, LogLevel |
| `src/MGG.Pulse.Domain/Ports/` | Create | IInputSimulator, IIdleDetector, ILoggerService, IConfigRepository, ITrayService |
| `src/MGG.Pulse.Application/Common/Result.cs` | Create | Result<T> record |
| `src/MGG.Pulse.Application/Rules/` | Create | IRule, IdleRule, AggressiveModeRule, IntervalRule, RuleEngine |
| `src/MGG.Pulse.Application/UseCases/` | Create | StartSimulationUseCase, StopSimulationUseCase |
| `src/MGG.Pulse.Application/Orchestration/CycleOrchestrator.cs` | Create | Loop principal |
| `src/MGG.Pulse.Infrastructure/Win32/` | Create | Win32InputSimulator, Win32IdleDetector |
| `src/MGG.Pulse.Infrastructure/Persistence/JsonConfigRepository.cs` | Create | JSON config adapter |
| `src/MGG.Pulse.Infrastructure/Logging/FileLoggerService.cs` | Create | Log rotativo |
| `src/MGG.Pulse.Infrastructure/Tray/SystemTrayService.cs` | Create | WinForms NotifyIcon en STA thread |
| `src/MGG.Pulse.UI/App.xaml.cs` | Create | Composition Root + lifecycle |
| `src/MGG.Pulse.UI/Windows/SplashWindow.xaml` | Create | Splash screen animado |
| `src/MGG.Pulse.UI/Windows/MainWindow.xaml` | Create | Ventana principal |
| `src/MGG.Pulse.UI/Views/MainPage.xaml` | Create | Dashboard, controles, config, logs |
| `src/MGG.Pulse.UI/ViewModels/MainViewModel.cs` | Create | ViewModel principal |
| `src/MGG.Pulse.UI/Themes/` | Create | ResourceDictionaries con color tokens |
| `tests/MGG.Pulse.Tests.Unit/` | Create | Tests de Domain + Application |

## Interfaces / Contracts

```csharp
// Port signatures
Task ExecuteAsync(InputType inputType, CancellationToken ct);    // IInputSimulator
TimeSpan GetIdleTime();                                           // IIdleDetector
Task LogAsync(LogLevel level, string message, CancellationToken ct); // ILoggerService
Task<SimulationConfig> LoadAsync(CancellationToken ct);          // IConfigRepository
Task SaveAsync(SimulationConfig config, CancellationToken ct);   // IConfigRepository
void Initialize(Action onShow, Action onStop, Action onExit);    // ITrayService
void SetTooltip(string text);                                     // ITrayService

// Rule contract
RuleResult Evaluate(SimulationContext context);                  // IRule
// RuleResult = record { bool ShouldExecute, string Reason, int Priority }
// SimulationContext = record { SimulationMode Mode, TimeSpan IdleTime, DateTime LastExecuted }
```

## Testing Strategy

| Layer | Qué testear | Approach |
|-------|-------------|----------|
| Unit — Domain | IntervalRange invariants, SimulationSession record actions | xUnit, sin mocks |
| Unit — Rules | IdleRule, AggressiveModeRule, IntervalRule contra cada scenario de spec | xUnit + Mock<IIdleDetector> |
| Unit — RuleEngine | Combinación de reglas, prioridad | xUnit + mocks |
| Unit — Use Cases | Start/Stop con configs válidas e inválidas, cancellation | xUnit + mocks completos |

Win32, WinUI 3, y WinForms NO se testean en unit — solo via uso real de la app.

## Migration / Rollout

No migration required — proyecto nuevo desde cero.

## Open Questions

- Ninguna — diseño completo y sin bloqueantes.
