# Architecture — MGG Pulse

> Companion to [`README.md`](./README.md). Covers layer structure, shell flow, update pipeline, and file map.

---

## Layer Overview

MGG Pulse follows **Hexagonal (Ports & Adapters) + MVVM** architecture. Dependencies flow strictly inward.

```
┌──────────────────────────────────────────────────────────────────┐
│  MGG.Pulse.UI  (WinUI 3 · MVVM · CommunityToolkit.Mvvm)         │
│  ShellPage · DashboardPage · SettingsPage · AboutPage            │
│  SplashWindow · MainWindow · ViewModels                          │
└─────────────────────┬────────────────────────────────────────────┘
                      │  Commands / Queries
                      ▼
┌──────────────────────────────────────────────────────────────────┐
│  MGG.Pulse.Application  (Use Cases · Rule Engine · Hosted Svcs)  │
│  StartSimulationUseCase  StopSimulationUseCase                   │
│  CheckForUpdateUseCase   UpdateHostedService                     │
│  RuleEngine (IdleRule · AggressiveModeRule · IntervalRule)       │
│  CycleOrchestrator                                               │
└───────────┬──────────────────────────────────────────────────────┘
            │  Ports (interfaces only)
            ▼
┌──────────────────────────────────────────────────────────────────┐
│  MGG.Pulse.Domain  (Entities · Value Objects · Enums · Ports)    │
│  Entities: SimulationConfig  SimulationState  SimulationSession  │
│  Value Objects: SimulationAction  IntervalRange                  │
│  Enums: SimulationMode  InputType  LogLevel                      │
│  Ports: IInputSimulator  IIdleDetector  IConfigRepository        │
│         ILoggerService  ITrayService  IUpdateService             │
└──────────────────────────────────────────────────────────────────┘
            ▲  implements ports
┌──────────────────────────────────────────────────────────────────┐
│  MGG.Pulse.Infrastructure  (Win32 · JSON · Tray · HTTP)          │
│  Win32InputSimulator (SendInput)                                 │
│  Win32IdleDetector   (GetLastInputInfo)                          │
│  JsonConfigRepository  FileLoggerService  SystemTrayService      │
│  GithubReleaseUpdateService  SystemTimeProvider                  │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│  MGG.Pulse.Tests.Unit  (xUnit + Moq)                             │
│  Tests Domain and Application layers only.                       │
│  Infrastructure and UI are NOT unit-tested (Win32 / WinUI 3).   │
└──────────────────────────────────────────────────────────────────┘
```

### Dependency Rule (enforced)

```
UI         → Application → Domain
Infrastructure           → Domain  (implements ports)
Tests.Unit → Application → Domain  (mocks Infrastructure)
```

**Domain MUST have zero external NuGet or project references.**

---

## Shell Navigation

The UI follows a three-level window hierarchy:

```
App.xaml.cs (Composition Root)
    │
    ├── 1. SplashWindow (borderless, centered, 5s minimum)
    │       └── Activates on startup
    │       └── Runs DI bootstrapping in parallel
    │       └── On complete → activates MainWindow, closes self
    │
    └── 2. MainWindow (root Frame)
            └── Frame navigates to → ShellPage
                    └── NavigationView with five destinations:
                            ├── DashboardPage  (simulation controls and runtime status)
                            ├── SettingsPage   (config: mode, interval, input type)
                            ├── AppearancePage (theme preference: Dark/Light/Auto + restart flow)
                            ├── LogsPage       (dedicated in-session log viewer)
                            └── AboutPage      (version, changelog link, manual update check)
```

### Window Lifecycle

```
App.OnLaunched()
  ├── Create SplashWindow → Activate()
  ├── Build IServiceProvider (DI)
  ├── Start UpdateHostedService
  └── Task.Run(InitAsync)
        ├── Load config (IConfigRepository)
        ├── Start ITrayService
        └── DispatcherQueue.TryEnqueue →
              ├── Create MainWindow → Activate()
              └── SplashWindow.Close()
```

---

## Update Pipeline

### Update Service Architecture

```
Domain:           IUpdateService
                       │
Application:      CheckForUpdateUseCase ──► Result<UpdateCheckResult>
                  UpdateHostedService   (startup + 4h timer)
                       │
Infrastructure:   GithubReleaseUpdateService
                       │  HttpClient GET
                       ▼
GitHub raw (main): latest.json
                  { "version", "url", "sha256", "notes" }
```

### Startup Update Check Sequence

```
App.xaml.cs        UpdateHostedService     GithubReleaseUpdateService    GitHub
     │                      │                          │                    │
     ├─ StartAsync() ───────▶│                          │                    │
     │                      ├── CheckForUpdateUseCase ─▶│                    │
     │                      │                          ├─── GET latest.json ─▶│
     │                      │                          │◄──── JSON ───────────┤
     │                      │◄── UpdateCheckResult ────┤                    │
     │                      │                          │                    │
     │          [if IsUpdateAvailable]                 │                    │
     │◄── (future: tray notification / badge) ─────────┤                    │
```

### Manual Update Check Sequence (from AboutPage)

```
AboutPage          AboutViewModel      CheckForUpdateUseCase    GitHub
    │                    │                      │                  │
    ├─ Button click ─────▶│                      │                  │
    │                    ├─ IsCheckingForUpdate = true              │
    │                    ├─ ExecuteAsync(manual=true) ─────────────▶│
    │                    │                      ├── GET latest.json ▶│
    │                    │                      │◄── JSON ───────────┤
    │                    │◄── Result<UpdateCheckResult> ────────────┤
    │                    ├─ UpdateStatusMessage = "v1.x available"  │
    │                    └─ IsCheckingForUpdate = false             │
```

### Installer Download & Silent Update Flow

```
AboutViewModel (or future UpdateDialog)
    │
    ├── IUpdateService.DownloadAndInstallUpdateAsync(url)
    │       ├── HttpClient → %TEMP%\MGGPulse-Setup-{version}.exe
    │       └── Process.Start("setup.exe", "/SILENT")
    │
    └── Application.Current.Exit()
            └── Inno Setup waits for lock release → installs over existing files
```

---

## Rule Engine

The simulation engine uses a **rule-based evaluation loop**, not a simple timer:

```
CycleOrchestrator  (Application layer)
    │
    ├── Reads SimulationConfig (from IConfigRepository)
    ├── Reads SimulationState  (current idle time, last action timestamp)
    │
    └── RuleEngine.Evaluate(context)
            ├── IdleRule        → allow only if idleTime > threshold
            ├── AggressiveModeRule → bypass idle check in Aggressive mode
            └── IntervalRule    → enforce min/max interval between actions
                    │
                    └── RuleResult { ShouldExecute, Reason, Priority }
                            │
                    [if ShouldExecute]
                            └── IInputSimulator.SimulateInput(SimulationAction)
```

---

## Build & Installer Pipeline

```
build/build.ps1 (from app/ directory)
    │
    ├── [1/4] dotnet publish MGG.Pulse.UI.csproj
    │           --runtime win-x64
    │           --self-contained false
    │           --output build/publish/
    │
    ├── [2/4] tools/gen-icon.ps1
    │           assets/branding/logo-main.png → assets/branding/icon.ico
    │           (16 / 32 / 48 / 256 px layers via ImageMagick)
    │
    ├── [3/4] Inno Setup (build/pulse.iss)
    │           Source:     build/publish/**
    │           Icon:       assets/branding/icon.ico
    │           Output:     build/output/MGGPulse-Setup-{version}.exe
    │           Install to: %LocalAppData%\MGG\Pulse\  (PrivilegesRequired=lowest)
    │           Tasks:      optional desktop icon, optional startup registry entry
    │
    └── [4/4] Done → build/output/MGGPulse-Setup-{version}.exe
```

### `latest.json` Schema (`main` branch manifest)

```json
{
  "version": "1.3.2",
  "url": "https://github.com/mgg-171093/mgg-pulse/releases/download/v1.3.2/MGGPulse-Setup-1.3.2.exe",
  "sha256": "<lowercase hex SHA-256 of the .exe>",
  "notes": "Short release notes shown to the user."
}
```

Commit `app/build/latest.json` on `main` for every release update. The auto-updater fetches it from the raw `main` branch URL. The installer `.exe` referenced by `url` remains hosted in GitHub Releases.

---

## Data Flow — Config Persistence

```
SettingsViewModel
    │
    └── IConfigRepository.SaveAsync(SimulationConfig)
            └── JsonConfigRepository (Infrastructure)
                    └── %AppData%\MGG\Pulse\config.json

App startup:
    └── IConfigRepository.LoadAsync()
            └── Deserializes config.json → SimulationConfig
                    └── Passed to CycleOrchestrator and ViewModels via DI
```

---

## Composition Root (DI)

All services are wired in `App.xaml.cs`:

```csharp
// Domain ports → Infrastructure adapters
services.AddSingleton<IInputSimulator, Win32InputSimulator>();
services.AddSingleton<IIdleDetector,   Win32IdleDetector>();
services.AddSingleton<IConfigRepository, JsonConfigRepository>();
services.AddSingleton<ILoggerService,  FileLoggerService>();
services.AddSingleton<ITrayService,    SystemTrayService>();
services.AddSingleton<IUpdateService,  GithubReleaseUpdateService>();

// Application layer
services.AddSingleton<RuleEngine>();
services.AddSingleton<CycleOrchestrator>();
services.AddSingleton<CheckForUpdateUseCase>();
services.AddSingleton<StartSimulationUseCase>();
services.AddSingleton<StopSimulationUseCase>();
services.AddSingleton<ITimeProvider,   SystemTimeProvider>();
services.AddHostedService<UpdateHostedService>();

// UI ViewModels
services.AddTransient<ShellViewModel>();
services.AddTransient<MainViewModel>();
services.AddTransient<SettingsViewModel>();
services.AddTransient<AboutViewModel>();
```

---

## File Map

```
mgg-pulse/
├── README.md                           ← Project overview, install, build, quick start
├── ARCHITECTURE.md                     ← This file
├── LICENSE
│
└── app/
    ├── MGG.Pulse.slnx                  ← Solution file
    ├── AGENTS.md                       ← AI agent critical rules and conventions
    ├── CHANGELOG.md                    ← Version history
    ├── Directory.Build.props           ← Shared MSBuild properties (TargetFramework, etc.)
    │
    ├── assets/branding/
    │   ├── banner-readme.png           ← GitHub README banner
    │   ├── logo-main.png               ← Primary app logo (splash screen, about)
    │   ├── logo-sidebar.png            ← Compact sidebar/tray logo variant
    │   ├── icon-app.png                ← App icon source (PNG)
    │   └── icon.ico                    ← Generated by tools/gen-icon.ps1
    │
    ├── build/
    │   ├── build.ps1                   ← Full release pipeline (publish → icon → Inno Setup)
    │   ├── pulse.iss                   ← Inno Setup installer script
    │   └── latest.json                 ← Update manifest schema (fill before each release)
    │
    ├── tools/
    │   └── gen-icon.ps1                ← Converts logo-main.png → icon.ico (ImageMagick)
    │
    ├── src/
    │   ├── MGG.Pulse.Domain/
    │   │   ├── Entities/               ← SimulationConfig, SimulationState, SimulationSession
    │   │   ├── ValueObjects/           ← SimulationAction, IntervalRange
    │   │   ├── Enums/                  ← SimulationMode, InputType, LogLevel
    │   │   ├── Ports/                  ← IInputSimulator, IIdleDetector, IConfigRepository,
    │   │   │                              ILoggerService, ITrayService, IUpdateService
    │   │   └── Updates/                ← UpdateManifest (domain model)
    │   │
    │   ├── MGG.Pulse.Application/
    │   │   ├── Abstractions/           ← ITimeProvider
    │   │   ├── Common/                 ← Result<T> pattern
    │   │   ├── Orchestration/          ← CycleOrchestrator
    │   │   ├── Rules/                  ← RuleEngine, IdleRule, AggressiveModeRule, IntervalRule
    │   │   ├── Updates/                ← CheckForUpdateUseCase, UpdateHostedService,
    │   │   │                              VersionComparer, ManifestValidator, UpdateCheckResult
    │   │   └── UseCases/               ← StartSimulationUseCase, StopSimulationUseCase
    │   │
    │   ├── MGG.Pulse.Infrastructure/
    │   │   ├── Logging/                ← FileLoggerService
    │   │   ├── Persistence/            ← JsonConfigRepository
    │   │   ├── Tray/                   ← SystemTrayService
    │   │   └── Update/                 ← GithubReleaseUpdateService, SystemTimeProvider
    │   │
    │   └── MGG.Pulse.UI/
    │       ├── App.xaml / .cs          ← Composition Root, splash lifecycle, DI
    │       ├── Converters/             ← BoolToVisibilityConverter
    │       ├── Diagnostics/            ← CrashLogger
    │       ├── Themes/                 ← DarkTheme.xaml, LightTheme.xaml
    │       ├── ViewModels/             ← ShellViewModel, MainViewModel, SettingsViewModel,
    │       │                              AboutViewModel
    │       ├── Views/                  ← ShellPage, DashboardPage, SettingsPage, AboutPage,
    │       │                              MainPage (legacy root)
    │       └── Windows/                ← MainWindow, SplashWindow
    │
    ├── tests/
    │   └── MGG.Pulse.Tests.Unit/
    │       └── Application/
    │           └── Updates/            ← CheckForUpdateUseCaseTests, UpdateManifestValidationTests,
    │                                      UpdateHostedServiceTests
    │
    └── openspec/                       ← SDD artifacts (specs, changes, config)
        ├── config.yaml
        ├── specs/                      ← Main/canonical specs
        └── changes/                    ← Active and archived change sets
```

---

## Threading Constraints

| Scenario | Rule |
|----------|------|
| Update check result → UI update | Must use `DispatcherQueue.TryEnqueue` |
| Background timer tick | Runs on `ThreadPool` — never touch UI directly |
| Installer download | `Task.Run` → background thread |
| App exit after update | `Application.Current.Exit()` on UI thread only |
| Win32 `SendInput` | Can be called from any thread |
| Win32 `GetLastInputInfo` | Can be called from any thread |

---

## Key Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Navigation framework | WinUI 3 `NavigationView` | Native Windows 11 styling, fits 3-page app, no over-engineering |
| Update service | `IUpdateService` port + `GithubReleaseUpdateService` adapter | Mockable, decoupled from UI, testable use case |
| Installer | `dotnet publish` + Inno Setup | Matches mgg-packify pattern; MSIX restricts background execution and file access |
| DI container | `Microsoft.Extensions.DependencyInjection` | Standard .NET — no extra dependency, familiar to all .NET devs |
| State management | CommunityToolkit.Mvvm source generators | Less boilerplate, same MVVM patterns, WinUI 3 compatible |
| Config format | JSON (`System.Text.Json`) | Human-editable, no migration tooling needed, survives upgrades |
| Update manifest | `latest.json` committed on `main` (raw URL) | Zero infrastructure — static file in repo, installer binary in GitHub Releases, same as mgg-packify |
