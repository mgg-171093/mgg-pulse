# MGG Pulse — Agent Instructions

## Project Overview

**MGG Pulse** is a Windows desktop application built with WinUI 3 (.NET 8).
It's a configurable, rule-based user activity simulator designed to prevent session timeouts in remote environments (RDP, VDI, Citrix, etc.).

The app runs silently in the background via System Tray and simulates minimal, non-intrusive user input only when the user is actually idle.

---

## Architecture

- **Pattern**: Hexagonal (Ports & Adapters) + MVVM
- **Paradigm**: Rule-based simulation engine (NOT a simple loop)
- **Layers**:
  - `MGG.Pulse.Domain` — Entities, Value Objects, Enums, Port interfaces. Zero external dependencies.
  - `MGG.Pulse.Application` — Use Cases, Rule Engine, Cycle Orchestrator. Depends only on Domain.
  - `MGG.Pulse.Infrastructure` — Win32 adapters, JSON config repo, file logger, tray service.
  - `MGG.Pulse.UI` — WinUI 3 views, ViewModels (CommunityToolkit.Mvvm), Composition Root.
  - `MGG.Pulse.Tests.Core` — xUnit + Moq CI-safe tests for Domain/Application/Infrastructure-safe logic.
  - `MGG.Pulse.Tests.UI` — xUnit + Moq local-only tests for UI/WinRT-bound logic.

### Dependency Rule (CRITICAL)
```
UI → Application → Domain
Infrastructure → Domain (implements ports)
Tests.Core → Domain + Application + Infrastructure (no UI references)
Tests.UI → UI + Domain + Application + Infrastructure (local-only)
```
**Domain MUST have zero references to any other project or NuGet package.**

---

## Key Domain Concepts

| Concept | Type | Description |
|---|---|---|
| `SimulationConfig` | Entity | User configuration (mode, interval, input type) |
| `SimulationState` | Entity | Current runtime state (running, idle time, last action) |
| `SimulationSession` | Entity | Represents one complete run session with audit trail |
| `SimulationAction` | Value Object | What input was simulated, when, triggered by which rule |
| `IntervalRange` | Value Object | Min/max interval in seconds |
| `SimulationMode` | Enum | Intelligent / Aggressive / Manual |
| `InputType` | Enum | Mouse / Keyboard / Combined |

## Ports (Interfaces — defined in Domain)

- `IInputSimulator` — abstracts Win32 SendInput
- `IIdleDetector` — returns `TimeSpan IdleTime` (abstracts GetLastInputInfo)
- `ILoggerService` — structured logging
- `IConfigRepository` — load/save SimulationConfig
- `ITrayService` — system tray icon, menu, notifications

## Rules

| Rule | Description |
|---|---|
| `IdleRule` | Only simulate if idle time exceeds threshold |
| `AggressiveModeRule` | Bypass idle check in Aggressive mode |
| `IntervalRule` | Enforce min/max timing between actions |

`RuleEngine` evaluates all rules and returns `RuleResult { ShouldExecute, Reason, Priority }`.

---

## Coding Conventions

- **Naming**: PascalCase for types/properties, `_camelCase` for private fields, `camelCase` for locals
- **Async**: Always use `CancellationToken` in Application layer async methods
- **Errors**: Use `Result<T>` pattern in Use Cases instead of exceptions for expected failures
- **DI**: `Microsoft.Extensions.DependencyInjection` wired in `App.xaml.cs` (Composition Root)
- **Commits**: Conventional Commits (`feat:`, `fix:`, `refactor:`, `test:`, `chore:`)
- **No mixed responsibilities**: never reference Infrastructure types from Application or Domain

---

## Project File Structure

```
mgg-pulse/
├── src/
│   ├── MGG.Pulse.Domain/
│   ├── MGG.Pulse.Application/
│   ├── MGG.Pulse.Infrastructure/
│   └── MGG.Pulse.UI/
├── tests/
│   ├── MGG.Pulse.Tests.Core/
│   └── MGG.Pulse.Tests.UI/
├── assets/
│   └── branding/
│       └── logo.png          ← REPLACE THIS to update the logo everywhere
├── openspec/                 ← SDD artifacts (specs, changes, config)
├── .atl/                     ← Agent tooling (skill registry)
├── .skills/                  ← Project-specific agent skills
├── AGENTS.md                 ← This file
└── README.md
```

### Logo Replacement
The single source of truth for the logo is:
```
assets/branding/logo.png
```
Replace that file to update the logo in the splash screen and tray icon.
The build copies it automatically — no other changes needed.

---

## UI Design System

### Spacing Scale
`4 / 8 / 16 / 24 / 32` px

### Border Radius
`8px` (CornerRadius="8")

### Color Tokens

| Token | Dark | Light |
|---|---|---|
| Background | `#0F111A` | `#F5F7FA` |
| Surface | `#1A1D2E` | `#FFFFFF` |
| SurfaceVariant | `#23263A` | `#EDEFF5` |
| Border | `#2A2E45` | `#D0D4E0` |
| TextPrimary | `#FFFFFF` | `#1A1A1A` |
| TextSecondary | `#B0B3C0` | `#5C5F70` |
| Primary | `#4CAF50` | same |
| PrimaryHover | `#66BB6A` | same |
| PrimaryActive | `#388E3C` | same |

Primary color used ONLY for: buttons, toggles, status indicators.

---

## Config Persistence

Config file location: `%AppData%\MGG\Pulse\config.json`

---

## SDD Workflow

This project uses **Spec-Driven Development** (openspec mode).

```
sdd-explore → sdd-propose → sdd-spec → sdd-design → sdd-tasks → sdd-apply → sdd-verify → sdd-archive
```

All SDD artifacts live in `openspec/`.

---

## Skills to Load (Auto-trigger)

| Context | Skill |
|---|---|
| Writing C# code, naming conventions | `.skills/csharp-conventions/SKILL.md` |
| WinUI 3 XAML, bindings, MVVM | `.skills/winui3-patterns/SKILL.md` |
| Writing xUnit tests, Moq | `.skills/xunit-testing/SKILL.md` |

Load skills BEFORE writing code. Apply ALL patterns.

---

## Out of Scope — V1

- MSIX installer / code signing
- Auto-update mechanism
- UI automation / E2E tests
- Multi-user / multi-profile support
