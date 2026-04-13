## Exploration: MGG Pulse — Project Foundation

### Current State

El proyecto está en estado cero — solo existe:
- `prompt-v1.txt` — prompt original de diseño
- `AGENTS.md` — instrucciones para agentes IA
- `openspec/config.yaml` — config SDD
- `assets/branding/logo.png` — logo provisional
- `.skills/*` — 3 skills del proyecto
- `README.md`

No hay código C# todavía. Este es el change fundacional: crear la solución .NET completa con todos los proyectos, implementar el dominio, la capa de aplicación, la infraestructura, la UI y los tests.

### Scope del Change

Este es un change grande pero cohesivo — crear todo desde cero. Lo dividimos en capas bien definidas:

**Capa 1 — Solución y estructura** (`MGG.Pulse.sln` + 5 `.csproj`)
**Capa 2 — Domain** (Entities, VOs, Enums, Ports)
**Capa 3 — Application** (Use Cases, Rules, RuleEngine, CycleOrchestrator, Result)
**Capa 4 — Infrastructure** (Win32InputSimulator, Win32IdleDetector, JsonConfigRepository, FileLoggerService, SystemTrayService)
**Capa 5 — UI** (SplashWindow, MainWindow, MainPage, ViewModel, Design System, Composition Root)
**Capa 6 — Tests.Unit** (Domain + Application tests)

### Decisiones de Arquitectura

#### Domain
```
Entities:
  SimulationConfig     — modo, intervalo, tipo de input, opciones stealth
  SimulationState      — estado en tiempo real (running, idleTime, lastAction, nextScheduled)
  SimulationSession    — audit trail de una sesión completa

Value Objects:
  IntervalRange        — min/max segundos, inmutable, validaciones
  SimulationAction     — qué input, cuándo, qué regla lo disparó

Enums:
  SimulationMode       — Intelligent | Aggressive | Manual
  InputType            — Mouse | Keyboard | Combined
  LogLevel             — Minimal | Normal | Verbose

Ports (Interfaces):
  IInputSimulator      — Execute(InputType, CancellationToken)
  IIdleDetector        — GetIdleTime() → TimeSpan
  ILoggerService       — LogAsync(LogLevel, string, CancellationToken)
  IConfigRepository    — LoadAsync / SaveAsync
  ITrayService         — Initialize, ShowNotification, SetTooltip
```

#### Application
```
IRule.Evaluate(SimulationContext) → RuleResult { ShouldExecute, Reason, Priority }

Rules:
  IdleRule             — bloquea si idle < threshold (Intelligent mode)
  AggressiveModeRule   — permite siempre (Aggressive mode)
  IntervalRule         — bloquea si tiempo desde última acción < min interval

RuleEngine:
  Evaluate(context) → RuleResult — aplica todas las reglas, retorna resultado final

CycleOrchestrator:
  RunAsync(config, CancellationToken) — loop principal con rules + simulation

Use Cases:
  StartSimulationUseCase  → Result<SimulationSession>
  StopSimulationUseCase   → Result<bool>
```

#### Infrastructure
```
Win32InputSimulator   — P/Invoke SendInput (mouse 1-2px, keyboard Shift/Ctrl)
Win32IdleDetector     — P/Invoke GetLastInputInfo
JsonConfigRepository  — System.Text.Json → %AppData%\MGG\Pulse\config.json
FileLoggerService     — log rotativo en %AppData%\MGG\Pulse\logs\
SystemTrayService     — WinForms NotifyIcon (más maduro que WinUI 3 tray)
```

**Decisión sobre System Tray**: WinForms NotifyIcon es la opción correcta para tray en .NET 8. El WinUI 3 nativo no tiene tray estable. Se corre en un thread STA separado.

#### UI
```
SplashWindow   — ventana custom sin title bar, logo + fade-in + progress bar
MainWindow     — ventana principal, 400×600, sin resize
MainPage       — Dashboard + Controls + Config + Logs tabs
MainViewModel  — CommunityToolkit.Mvvm, ObservableObject
App.xaml.cs    — Composition Root, DI container, lifecycle
```

#### Splash Screen técnico
- Ventana WinUI 3 sin chrome (ExtendedTitleBar)
- Logo fade-in via Storyboard (Opacity 0→1, 800ms)
- Barra de progreso determinística (init services step by step)
- Desaparece y muestra MainWindow (o tray si StartMinimized)

### Enfoque Recomendado

**Un solo change** `ch-001-project-foundation` que construye todo de abajo para arriba:
1. Estructura de solución .NET
2. Domain (pure, sin dependencias)
3. Application (depende solo de Domain)
4. Infrastructure (implementa ports del Domain)
5. UI (WinUI 3, MVVM, Design System)
6. Tests.Unit (Domain + Application)

Este orden garantiza que en cada paso el código compila y los tests pasan.

### Riesgos

- **Win32 P/Invoke**: `SendInput` y `GetLastInputInfo` son straightforward pero hay que manejar UAC (elevated process) — en V1 asumimos no-elevated
- **WinForms + WinUI 3**: correr `NotifyIcon` en thread STA separado es un patrón conocido pero requiere sincronización cuidadosa con el UI thread de WinUI 3
- **CommunityToolkit.Mvvm source generators**: requieren que el proyecto sea `partial class` — convención estricta
- **AppWindow API**: el centrado del SplashScreen usa `DisplayArea` que requiere Windows App SDK 1.3+

### Ready for Proposal
Sí — contexto completo para proceder a sdd-propose.
